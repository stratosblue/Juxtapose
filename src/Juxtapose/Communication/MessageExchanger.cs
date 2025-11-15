using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Juxtapose;

/// <inheritdoc cref="IMessageExchanger"/>
public class MessageExchanger : KeepRunningObject, IMessageExchanger
{
    #region Public 事件

    /// <inheritdoc/>
    public event Action<object>? OnInvalid;

    #endregion Public 事件

    #region Private 字段

    private readonly ICommunicationChannel _communicationChannel;

    private readonly ICommunicationFrameCodec _frameCodec;

    private readonly ILogger _logger;

    private readonly ICommunicationMessageCodec _messageCodec;

    private readonly Channel<object> _messageReceivingChannel;

    private readonly SemaphoreSlim _messageWriteSemaphore;

    private readonly object _syncRoot = new();

    private CancellationTokenSource? _channelCancellationTokenSource;

    private PipeReader? _pipeReader;

    private PipeWriter? _pipeWriter;

    #endregion Private 字段

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="MessageExchanger"/>
    /// </summary>
    /// <param name="communicationChannel"></param>
    /// <param name="frameCodec"></param>
    /// <param name="messageCodec"></param>
    /// <param name="loggerFactory"></param>
    public MessageExchanger(ICommunicationChannel communicationChannel, ICommunicationFrameCodec frameCodec, ICommunicationMessageCodec messageCodec, ILoggerFactory loggerFactory)
    {
        _communicationChannel = communicationChannel ?? throw new ArgumentNullException(nameof(communicationChannel));
        _frameCodec = frameCodec ?? throw new ArgumentNullException(nameof(frameCodec));
        _messageCodec = messageCodec ?? throw new ArgumentNullException(nameof(messageCodec));
        _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger("Juxtapose.MessageExchanger");
        _messageReceivingChannel = Channel.CreateUnbounded<object>();

        communicationChannel.RegisterOnInitializationConnected(OnCommunicationChannelConnected);

        _messageWriteSemaphore = new SemaphoreSlim(1, 1);
    }

    #endregion Public 构造函数

    #region Private 方法

    private void DisposeChannelCancellationTokenSource()
    {
        if (_channelCancellationTokenSource is CancellationTokenSource tokenSource)
        {
            _channelCancellationTokenSource = null;
            tokenSource.Cancel();
            tokenSource.Dispose();
        }
    }

    private async Task MessageReceiveLoopAsync(CancellationToken cancellation)
    {
        var reader = _pipeReader!;

        ThrowIfChannelNotReady(reader);

        while (!cancellation.IsCancellationRequested)
        {
            var readResult = await reader.ReadAsync(cancellation);

            var buffer = readResult.Buffer;

            while (_frameCodec.TryGetMessageFrame(buffer, out var frameBuffer))
            {
                buffer = buffer.Slice(frameBuffer.Value.End);
                var message = _messageCodec.Decode(frameBuffer.Value);

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Message received: {Message}", message);
                }

                await _messageReceivingChannel.Writer.WriteAsync(message, RunningToken);
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (readResult.IsCompleted)
            {
                break;
            }
        }
    }

    private void OnCommunicationChannelConnected(Stream stream)
    {
        if (IsDisposed)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(stream);

        CancellationTokenSource tokenSource;

        lock (_syncRoot)
        {
            DisposeChannelCancellationTokenSource();
            tokenSource = CancellationTokenSource.CreateLinkedTokenSource(RunningToken);
            _channelCancellationTokenSource = tokenSource;
        }

        _pipeReader = PipeReader.Create(stream);
        _pipeWriter = PipeWriter.Create(stream);

        Task.Run(async () =>
        {
            try
            {
                await MessageReceiveLoopAsync(tokenSource.Token);
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                {
                    _logger.LogCritical(ex, "Message Receive Loop Error!");
                    Dispose();
                    throw;
                }
            }
        }, RunningToken);
    }

    private void ThrowIfChannelNotReady(object? channelObject)
    {
        if (channelObject is null)
        {
            throw new ChannelException("Channel is not ready yet.");
        }
        ThrowIfDisposed();
    }

    #endregion Private 方法

    #region Protected 方法

    /// <inheritdoc/>
    protected override bool Dispose(bool disposing)
    {
        if (base.Dispose(disposing))
        {
            OnInvalid?.Invoke(this);

            _communicationChannel.Dispose();
            DisposeChannelCancellationTokenSource();
            _messageWriteSemaphore.Dispose();

            return true;
        }
        return false;
    }

    #endregion Protected 方法

    #region Public 方法

    /// <inheritdoc/>
    public async IAsyncEnumerable<object> GetMessagesAsync([EnumeratorCancellation] CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            ThrowIfDisposed();
            yield return await _messageReceivingChannel.Reader.ReadAsync(cancellation);
        }
    }

    /// <inheritdoc/>
    public Task InitializationAsync(CancellationToken initializationToken)
    {
        return _communicationChannel.InitializationAsync(initializationToken);
    }

    /// <inheritdoc/>
    public virtual async Task WriteMessageAsync(object message, CancellationToken cancellation)
    {
        var writer = _pipeWriter!;

        ThrowIfChannelNotReady(writer);

        var localBufferWriter = new ArrayBufferWriter<byte>(1024);
        await _messageCodec.Encode(message, localBufferWriter);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Message waiting for write: {Message}", message);
        }

        await _messageWriteSemaphore.WaitAsync(cancellation);
        try
        {
            await _frameCodec.WriteMessageFrameAsync(writer, localBufferWriter.WrittenMemory);
            await writer.FlushAsync(cancellation);
        }
        catch
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            throw;
        }
        finally
        {
            _messageWriteSemaphore.Release();
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Message written: {Message}", message);
        }
    }

    #endregion Public 方法
}
