using Juxtapose.Messages;

namespace Juxtapose;

/// <summary>
///
/// </summary>
public static class MessageDispatcherExtensions
{
    #region Public 方法

    private static void ThrowIfDispatcherIsExecutorAndProcessNotAlive(MessageDispatcher dispatcher, Exception threwException)
    {
        if (dispatcher is not JuxtaposeExecutor executor)
        {
            return;
        }
        int? processId = null;
        int? exitCode = null;
        try
        {
            if (executor.TryGetExternalProcess(out var externalProcess)
                && !externalProcess.IsAlive)
            {
                processId = externalProcess.Id;
                exitCode = externalProcess.ExitCode;
            }
        }
        catch (Exception ex)
        {
            var exception = new JuxtaposeException("Juxtapose fail to pretty exception. RawException is in the exception Data.", ex);
            exception.Data.Add("RawException", threwException);
            throw exception;
        }

        if (processId.HasValue
            && exitCode.HasValue)
        {
            throw new ExternalProcessExitedException(processId.Value, exitCode.Value, $"Exception threw at remote call. And the external process 【{processId.Value}】 exited with code:【{exitCode.Value}】.", threwException);
        }
    }

    #region Async

    /// <summary>
    /// 发送执行实例方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <param name="dispatcher"></param>
    /// <param name="parameterPack"></param>
    /// <param name="instanceId"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static async Task<JuxtaposeAckMessage> InvokeInstanceMethodMessageAsync<TParameterPack>(this MessageDispatcher dispatcher,
                                                                                                   TParameterPack parameterPack,
                                                                                                   int instanceId,
                                                                                                   int commandId,
                                                                                                   CancellationToken cancellation = default)
    {
        var message = new InstanceMethodInvokeMessage<TParameterPack>(instanceId, commandId) { ParameterPack = parameterPack };
        try
        {
            return await dispatcher.InvokeMessageAsync(message, cancellation);
        }
        catch (JuxtaposeException) { throw; }
        catch (Exception ex)
        {
            ThrowIfDispatcherIsExecutorAndProcessNotAlive(dispatcher, ex);
            throw;
        }
    }

    /// <summary>
    /// 发送执行实例方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <typeparam name="TResultPack"></typeparam>
    /// <param name="dispatcher"></param>
    /// <param name="parameterPack"></param>
    /// <param name="instanceId"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static async Task<TResultPack> InvokeInstanceMethodMessageAsync<TParameterPack, TResultPack>(this MessageDispatcher dispatcher,
                                                                                                        TParameterPack parameterPack,
                                                                                                        int instanceId,
                                                                                                        int commandId,
                                                                                                        CancellationToken cancellation = default)
    {
        var message = new InstanceMethodInvokeMessage<TParameterPack>(instanceId, commandId) { ParameterPack = parameterPack };
        JuxtaposeAckMessage ackMessage;

        try
        {
            ackMessage = await dispatcher.InvokeMessageAsync(message, cancellation);
        }
        catch (JuxtaposeException) { throw; }
        catch (Exception ex)
        {
            ThrowIfDispatcherIsExecutorAndProcessNotAlive(dispatcher, ex);
            throw;
        }

        return ((InstanceMethodInvokeResultMessage<TResultPack>)ackMessage).Result!;
    }

    /// <summary>
    /// 发送执行实例方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <param name="dispatcher"></param>
    /// <param name="parameterPack"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static async Task<JuxtaposeAckMessage> InvokeStaticMethodMessageAsync<TParameterPack>(this MessageDispatcher dispatcher,
                                                                                                 TParameterPack parameterPack,
                                                                                                 int commandId,
                                                                                                 CancellationToken cancellation = default)
    {
        var message = new StaticMethodInvokeMessage<TParameterPack>(commandId) { ParameterPack = parameterPack };
        try
        {
            return await dispatcher.InvokeMessageAsync(message, cancellation);
        }
        catch (JuxtaposeException) { throw; }
        catch (Exception ex)
        {
            ThrowIfDispatcherIsExecutorAndProcessNotAlive(dispatcher, ex);
            throw;
        }
    }

    /// <summary>
    /// 发送执行实例方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <typeparam name="TResultPack"></typeparam>
    /// <param name="dispatcher"></param>
    /// <param name="parameterPack"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static async Task<TResultPack> InvokeStaticMethodMessageAsync<TParameterPack, TResultPack>(this MessageDispatcher dispatcher,
                                                                                                      TParameterPack parameterPack,
                                                                                                      int commandId,
                                                                                                      CancellationToken cancellation = default)
    {
        var message = new StaticMethodInvokeMessage<TParameterPack>(commandId) { ParameterPack = parameterPack };
        JuxtaposeAckMessage ackMessage;
        try
        {
            ackMessage = await dispatcher.InvokeMessageAsync(message, cancellation);
        }
        catch (JuxtaposeException) { throw; }
        catch (Exception ex)
        {
            ThrowIfDispatcherIsExecutorAndProcessNotAlive(dispatcher, ex);
            throw;
        }
        return ((StaticMethodInvokeResultMessage<TResultPack>)ackMessage).Result!;
    }

    #endregion Async

    #region Sync

    private static async Task<TResult> InvokeWithoutSyncContextAsync<TResult>(Func<Task<TResult>> action)
    {
        await SynchronizationContextRemover.Awaiter;
        return await action();
    }

    /// <summary>
    /// 发送执行实例方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <param name="dispatcher"></param>
    /// <param name="parameterPack"></param>
    /// <param name="instanceId"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static JuxtaposeAckMessage InvokeInstanceMethodMessage<TParameterPack>(this MessageDispatcher dispatcher,
                                                                                  TParameterPack parameterPack,
                                                                                  int instanceId,
                                                                                  int commandId,
                                                                                  CancellationToken cancellation = default)
    {
        return InvokeWithoutSyncContextAsync(() => dispatcher.InvokeInstanceMethodMessageAsync(parameterPack, instanceId, commandId, cancellation)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 发送执行实例方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <typeparam name="TResultPack"></typeparam>
    /// <param name="dispatcher"></param>
    /// <param name="parameterPack"></param>
    /// <param name="instanceId"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>

    public static TResultPack InvokeInstanceMethodMessage<TParameterPack, TResultPack>(this MessageDispatcher dispatcher,
                                                                                       TParameterPack parameterPack,
                                                                                       int instanceId,
                                                                                       int commandId,
                                                                                       CancellationToken cancellation = default)
    {
        return InvokeWithoutSyncContextAsync(() => dispatcher.InvokeInstanceMethodMessageAsync<TParameterPack, TResultPack>(parameterPack, instanceId, commandId, cancellation)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 发送执行实例方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <param name="dispatcher"></param>
    /// <param name="parameterPack"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static JuxtaposeAckMessage InvokeStaticMethodMessage<TParameterPack>(this MessageDispatcher dispatcher,
                                                                                TParameterPack parameterPack,
                                                                                int commandId,
                                                                                CancellationToken cancellation = default)
    {
        return InvokeWithoutSyncContextAsync(() => dispatcher.InvokeStaticMethodMessageAsync<TParameterPack>(parameterPack, commandId, cancellation)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 发送执行实例方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <typeparam name="TResultPack"></typeparam>
    /// <param name="dispatcher"></param>
    /// <param name="parameterPack"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static TResultPack InvokeStaticMethodMessage<TParameterPack, TResultPack>(this MessageDispatcher dispatcher,
                                                                                     TParameterPack parameterPack,
                                                                                     int commandId,
                                                                                     CancellationToken cancellation = default)
    {
        return InvokeWithoutSyncContextAsync(() => dispatcher.InvokeStaticMethodMessageAsync<TParameterPack, TResultPack>(parameterPack, commandId, cancellation)).GetAwaiter().GetResult();
    }

    #endregion Sync

    #endregion Public 方法
}
