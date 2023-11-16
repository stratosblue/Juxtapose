using System.IO.Pipes;

namespace Juxtapose.Communication.Channel;

/// <summary>
/// <inheritdoc cref="NamedPipeCommunicationChannel"/> - 客户端
/// </summary>
public class NamedPipeCommunicationClient : NamedPipeCommunicationChannel, ICommunicationClient
{
    #region Private 字段

    private readonly NamedPipeClientStream _namedPipeClientStream;

    #endregion Private 字段

    #region Protected 属性

    /// <inheritdoc/>
    protected override bool IsReady => _namedPipeClientStream.IsConnected;

    #endregion Protected 属性

    #region Private 构造函数

    private NamedPipeCommunicationClient(NamedPipeClientStream namedPipeClientStream) : base(namedPipeClientStream)
    {
        _namedPipeClientStream = namedPipeClientStream;
    }

    #endregion Private 构造函数

    #region Public 构造函数

    /// <inheritdoc cref="NamedPipeCommunicationClient"/>
    public NamedPipeCommunicationClient(string pipeName, string? serverName = ".") : this(CreateNamedPipeClientStream(pipeName, serverName))
    {
    }

    #endregion Public 构造函数

    #region Private 方法

    private static NamedPipeClientStream CreateNamedPipeClientStream(string pipeName, string? serverName)
    {
        if (string.IsNullOrWhiteSpace(pipeName))
        {
            throw new ArgumentException($"“{nameof(pipeName)}”不能为 null 或空白。", nameof(pipeName));
        }

        return new NamedPipeClientStream(serverName ?? ".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
    }

    #endregion Private 方法

    #region Protected 方法

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _namedPipeClientStream.Dispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override Task WaitForConnectingAsync(CancellationToken token) => _namedPipeClientStream.ConnectAsync(token);

    #endregion Protected 方法
}
