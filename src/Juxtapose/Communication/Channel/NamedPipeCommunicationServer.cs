using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.Communication.Channel;

/// <summary>
/// <inheritdoc cref="NamedPipeCommunicationChannel"/> - 服务端
/// </summary>
public class NamedPipeCommunicationServer : NamedPipeCommunicationChannel, ICommunicationServer
{
    #region Private 字段

    private readonly NamedPipeServerStream _namedPipeServerStream;

    #endregion Private 字段

    #region Protected 属性

    /// <inheritdoc/>
    protected override bool IsReady => _namedPipeServerStream.IsConnected;

    #endregion Protected 属性

    #region Private 构造函数

    private NamedPipeCommunicationServer(NamedPipeServerStream namedPipeServerStream) : base(namedPipeServerStream)
    {
        _namedPipeServerStream = namedPipeServerStream;
    }

    #endregion Private 构造函数

    #region Public 构造函数

    /// <inheritdoc cref="NamedPipeCommunicationServer"/>
    public NamedPipeCommunicationServer(string pipeName) : this(CreateNamedPipeServerStream(pipeName))
    {
    }

    #endregion Public 构造函数

    #region Private 方法

    private static NamedPipeServerStream CreateNamedPipeServerStream(string pipeName)
    {
        if (string.IsNullOrWhiteSpace(pipeName))
        {
            throw new ArgumentException($"“{nameof(pipeName)}”不能为 null 或空白。", nameof(pipeName));
        }

        return new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
    }

    #endregion Private 方法

    #region Protected 方法

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _namedPipeServerStream.Dispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override Task WaitForConnectingAsync(CancellationToken token) => _namedPipeServerStream.WaitForConnectionAsync(token);

    #endregion Protected 方法
}