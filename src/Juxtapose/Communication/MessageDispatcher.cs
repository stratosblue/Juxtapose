using System.Collections.Concurrent;

using Juxtapose.Messages;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Juxtapose;

/// <summary>
/// 消息调度器
/// </summary>
public abstract class MessageDispatcher : KeepRunningObject, IInitializationable, IDisposable
{
    #region Protected 字段

    /// <inheritdoc cref="IMessageExchanger"/>
    protected readonly IMessageExchanger MessageExchanger;

    /// <summary>
    /// 消息的<see cref="TaskCompletionSource"/>
    /// </summary>
    protected internal readonly ConcurrentDictionary<int, TaskCompletionSource<JuxtaposeAckMessage>> MessageTaskCompletionSources = new();

    #endregion Protected 字段

    #region Protected 属性

    /// <summary>
    /// <see cref="ILogger"/>
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// 消息<inheritdoc cref="IUniqueIdGenerator"/>
    /// </summary>
    public IUniqueIdGenerator MessageIdGenerator { get; }

    #endregion Protected 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="MessageDispatcher"/>
    /// </summary>
    /// <param name="messageExchanger"></param>
    /// <param name="logger"></param>
    public MessageDispatcher(IMessageExchanger messageExchanger, ILogger logger)
    {
        MessageExchanger = messageExchanger ?? throw new ArgumentNullException(nameof(messageExchanger));
        Logger = logger ?? NullLogger.Instance;
        MessageIdGenerator = new CheckedIdGenerator(MessageTaskCompletionSources.ContainsKey);
    }

    #endregion Public 构造函数

    #region Private 方法

    private async Task InvokeProcessLoopAsync()
    {
        await foreach (var message in MessageExchanger.GetMessagesAsync(RunningToken))
        {
            if (message is JuxtaposeMessage juxtaposeMessage)
            {
                if (juxtaposeMessage.Id < 1)
                {
                    throw new JuxtaposeException($"Error message {nameof(JuxtaposeMessage.Id)} {juxtaposeMessage.Id}.");
                }
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var responseMessage = await OnMessageAsync(juxtaposeMessage, RunningToken);
                        if (responseMessage is null)
                        {
                            if (message is not JuxtaposeAckMessage)
                            {
                                await MessageExchanger.WriteMessageAsync(new JuxtaposeAckMessage(juxtaposeMessage.Id) { Id = MessageIdGenerator.Next() }, RunningToken);
                            }
                        }
                        else
                        {
                            //HACK 直接强制重新设置ID，有一点点暴力
                            responseMessage.Id = MessageIdGenerator.Next();

                            //HACK 保证顺序？保证正确性？
                            await MessageExchanger.WriteMessageAsync(responseMessage, RunningToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogDebug(ex, "Exection has threw at message process - {Message}", message);
                        var originExceptionType = ex is OperationCanceledException
                                                  ? Constants.OperationCanceledExceptionFullType
                                                  : ex.GetType().ToString();

                        await MessageExchanger.WriteMessageAsync(new ExceptionMessage(juxtaposeMessage.Id, originExceptionType, ex.Message, ex.StackTrace, ex.ToString()) { Id = MessageIdGenerator.Next() }, RunningToken);
                    }
                });
            }
            else
            {
                Dispose();
                throw new UnknownMessageException(message);
            }
        }
    }

    #endregion Private 方法

    #region Protected 方法

    /// <inheritdoc/>
    protected override bool Dispose(bool disposing)
    {
        if (base.Dispose(disposing))
        {
            MessageExchanger.Dispose();

            if (MessageTaskCompletionSources.Values is IEnumerable<TaskCompletionSource<JuxtaposeAckMessage>> completionSources
                && completionSources.Any())
            {
                Task.Run(() =>
                {
                    var ex = new ObjectDisposedException(nameof(MessageDispatcher));
                    foreach (var item in completionSources)
                    {
                        item.TrySetException(ex);
                    }
                });
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 接收到消息时
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    /// <exception cref="UnknownMessageException"></exception>
    protected virtual Task<JuxtaposeMessage?> OnMessageAsync(JuxtaposeMessage message, CancellationToken cancellation)
    {
        switch (message)
        {
            case JuxtaposeAckMessage juxtaposeAckMessage:
                if (MessageTaskCompletionSources.TryGetValue(juxtaposeAckMessage.AckId, out var taskCompletionSource))
                {
                    if (!taskCompletionSource.TrySetResult(juxtaposeAckMessage))
                    {
                        Logger.LogWarning("Received ack message {AckId} - {Message}. But set result failed.", juxtaposeAckMessage.AckId, juxtaposeAckMessage);
                    }
                }
                else
                {
                    Logger.LogWarning("Received ack message {AckId} - {Message}. But no waiter found.", juxtaposeAckMessage.AckId, juxtaposeAckMessage);
                }
                break;

            default:
                //HACK 此时终止对象?
                throw new UnknownMessageException(message);
        }
        return Task.FromResult<JuxtaposeMessage?>(null);
    }

    #endregion Protected 方法

    #region Public 方法

    /// <inheritdoc/>
    public virtual async Task InitializationAsync(CancellationToken initializationToken)
    {
        await MessageExchanger.InitializationAsync(initializationToken);

        _ = Task.Run(InvokeProcessLoopAsync, RunningToken);
    }

    /// <summary>
    /// 执行消息并等待返回
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public async Task<JuxtaposeAckMessage> InvokeMessageAsync(JuxtaposeMessage message, CancellationToken cancellation)
    {
        ThrowIfDisposed();

        cancellation.ThrowIfCancellationRequested();

        cancellation = cancellation.CanBeCanceled
                       ? cancellation
                       : RunningToken;

        //HACK 直接强制重新设置ID，有一点点暴力
        var id = MessageIdGenerator.Next();
        message.Id = id;

        try
        {
            Logger.LogTrace("Start invoke message : {Message}", message);

            var taskCompletionSource = new TaskCompletionSource<JuxtaposeAckMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            MessageTaskCompletionSources.TryAdd(id, taskCompletionSource);
            await MessageExchanger.WriteMessageAsync(message, cancellation);

            var resultReceiveTask = taskCompletionSource.Task;

            using var localCts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            if (await Task.WhenAny(resultReceiveTask, Task.Delay(Timeout.Infinite, localCts.Token)) == resultReceiveTask)
            {
                localCts.Cancel();

                var resultMessage = resultReceiveTask.IsCompleted
                                    ? resultReceiveTask.Result
                                    : await resultReceiveTask;
                if (resultMessage is ExceptionMessage exceptionMessage)
                {
                    if (Constants.OperationCanceledExceptionFullType.Equals(exceptionMessage.OriginExceptionType, StringComparison.Ordinal))
                    {
                        Logger.LogTrace("Remote OperationCanceledException has been received for message id {MessageId}.", message.Id);
                        throw new OperationCanceledException();
                    }
                    else
                    {
                        Logger.LogTrace("ExceptionMessage has been received for message id {MessageId}. OriginMessage: {OriginMessage} . OriginStackTrace: {OriginStackTrace}", message.Id, exceptionMessage.OriginMessage, exceptionMessage.OriginStackTrace);
                        throw new JuxtaposeRemoteException(originStackTrace: exceptionMessage.OriginStackTrace,
                                                           originMessage: exceptionMessage.OriginMessage,
                                                           originToStringValue: exceptionMessage.OriginToStringValue,
                                                           originExceptionType: exceptionMessage.OriginExceptionType);
                    }
                }
                return resultMessage;
            }
            else
            {
                ThrowIfDisposed();
                throw new OperationCanceledException(cancellation);
            }
        }
        finally
        {
            MessageTaskCompletionSources.TryRemove(id, out _);
        }
    }

    #endregion Public 方法
}
