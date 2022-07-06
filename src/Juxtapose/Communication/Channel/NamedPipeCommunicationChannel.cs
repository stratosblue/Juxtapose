using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Juxtapose.Internal;

namespace Juxtapose.Communication.Channel;

/// <summary>
/// 基于命名管道的<inheritdoc cref="ICommunicationChannel"/>
/// </summary>
public abstract class NamedPipeCommunicationChannel : ICommunicationChannel
{
    #region Private 字段

    private readonly PipeStream _pipeStream;
    private bool _isDisposed;
    private volatile bool _isInitialized;
    private Action<Stream>? _onConnectedCallback;

    #endregion Private 字段

    #region Protected 属性

    /// <summary>
    /// 是否已就绪
    /// </summary>
    protected abstract bool IsReady { get; }

    #endregion Protected 属性

    #region Public 构造函数

    /// <inheritdoc cref="NamedPipeCommunicationChannel"/>
    public NamedPipeCommunicationChannel(PipeStream pipeStream)
    {
        _pipeStream = pipeStream ?? throw new ArgumentNullException(nameof(pipeStream));
    }

    #endregion Public 构造函数

    #region Private 方法

    private async Task WaitReadyAsync(CancellationToken token)
    {
        while (!IsReady
               && !token.IsCancellationRequested)
        {
            await Task.Delay(50, token);
        }
    }

    #endregion Private 方法

    #region Protected 方法

    /// <summary>
    /// 等待连接
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    protected abstract Task WaitForConnectingAsync(CancellationToken token);

    #endregion Protected 方法

    #region Public 方法

    /// <inheritdoc/>
    public async Task InitializationAsync(CancellationToken initializationToken)
    {
        if (_isInitialized)
        {
            throw new ChannelException($"{nameof(NamedPipeCommunicationChannel)}.{nameof(InitializationAsync)} has been called.");
        }
        _isInitialized = true;

        await WaitForConnectingAsync(initializationToken);
        await WaitReadyAsync(initializationToken);

        _onConnectedCallback?.Invoke(_pipeStream);
    }

    /// <inheritdoc/>
    public IDisposable RegisterOnInitializationConnected(Action<Stream> onConnectedCallback)
    {
        _onConnectedCallback += onConnectedCallback;
        return new ActionDisposer(() =>
        {
            _onConnectedCallback -= onConnectedCallback;
        });
    }

    #endregion Public 方法

    #region Dispose

    /// <summary>
    ///
    /// </summary>
    ~NamedPipeCommunicationChannel()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _pipeStream.Dispose();
            _isDisposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion Dispose
}