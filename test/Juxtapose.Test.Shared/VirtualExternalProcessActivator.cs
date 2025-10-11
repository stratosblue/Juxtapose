using System.Text;
using Juxtapose.Utils;

namespace Juxtapose.Test;

internal class VirtualExternalProcessActivator : IExternalProcessActivator
{
    #region Public 方法

    public IExternalProcess CreateProcess(IJuxtaposeOptions options)
    {
        options.SessionId = Guid.NewGuid().ToString("N");

        return new VirtualExternalProcess(options);
    }

    public void Dispose()
    {
    }

    #endregion Public 方法
}

internal class VirtualExternalProcess : IExternalProcess
{
    #region Private 字段

    private readonly MemoryStream _memoryStream = new();

    private readonly CancellationTokenSource _runningCts = new();

    private bool _disposed;

    #endregion Private 字段

    #region Public 事件

    public event Action<IExternalProcess> OnProcessInvalid;

    #endregion Public 事件

    #region Public 属性

    public int ExitCode { get; } = 0;

    public bool HasExited { get; } = false;

    public int Id { get; } = 1;

    public bool IsAlive { get; } = true;

    public IJuxtaposeOptions Options { get; }

    public DateTime StartTime { get; } = DateTime.UtcNow;

    #endregion Public 属性

    #region Public 构造函数

    public VirtualExternalProcess(IJuxtaposeOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _memoryStream.Write(Encoding.UTF8.GetBytes("Disposed"));
        _memoryStream.Flush();
        _memoryStream.Dispose();
        _runningCts.Cancel();
        _runningCts.Dispose();
        OnProcessInvalid?.Invoke(this);
    }

    public long? GetMemoryUsage()
    {
        return null;
    }

    public StreamReader GetStandardError()
    {
        return null;
    }

    public StreamReader GetStandardOutput()
    {
        return null;
    }

    public Task InitializationAsync(CancellationToken initializationToken)
    {
        var args = ExternalProcessArgumentUtil.BuildJuxtaposeArgument(Options);
        _ = JuxtaposeEntryPoint.AsEndpointAsync(args.Split(' '), new ReflectionInitializationContextLoader(false), _runningCts.Token);
        return Task.CompletedTask;
    }

    #endregion Public 方法
}
