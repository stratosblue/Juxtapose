using System.Diagnostics;

namespace Juxtapose;

/// <summary>
/// 保持运行的对象
/// </summary>
public abstract class KeepRunningObject : IDisposable
{
    #region Private 字段

    private readonly CancellationTokenSource _runningCancellationTokenSource;

    private volatile bool _isDisposed;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 是否已释放
    /// </summary>
    public bool IsDisposed { [DebuggerStepThrough] get => _isDisposed; [DebuggerStepThrough] private set => _isDisposed = value; }

    /// <summary>
    /// 运行的Token
    /// </summary>
    public CancellationToken RunningToken { [DebuggerStepThrough] get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="KeepRunningObject"/>
    [DebuggerStepThrough]
    public KeepRunningObject()
    {
        _runningCancellationTokenSource = new();
        RunningToken = _runningCancellationTokenSource.Token;
    }

    #endregion Public 构造函数

    #region Protected 方法

    /// <summary>
    /// 检查是否已释放
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    #endregion Protected 方法

    #region Dispose

    /// <summary>
    ///
    /// </summary>
    ~KeepRunningObject()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// <inheritdoc cref="Dispose()"/>
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual bool Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            try
            {
                _runningCancellationTokenSource.Cancel();
            }
            catch { }
            finally
            {
                _runningCancellationTokenSource.Dispose();
            }
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion Dispose
}
