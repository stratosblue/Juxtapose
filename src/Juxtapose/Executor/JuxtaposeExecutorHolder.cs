namespace Juxtapose;

/// <summary>
/// 无持有限制的 <see cref="JuxtaposeExecutor"/> 持有器
/// </summary>
public abstract class JuxtaposeExecutorHolder(JuxtaposeExecutor executor) : IJuxtaposeExecutorHolder
{
    #region Private 字段

    private int _count = 0;

    private bool _isDisposed = false;

    private volatile bool _prepareDrop = false;

    #endregion Private 字段

    #region Protected 字段

    /// <summary>
    /// 同步根
    /// </summary>
    protected readonly object SyncRoot = new();

    #endregion Protected 字段

    #region Public 属性

    /// <inheritdoc/>
    public int Count => _count;

    /// <inheritdoc/>
    public JuxtaposeExecutor Executor { get; } = executor ?? throw new ArgumentNullException(nameof(executor));

    /// <inheritdoc/>
    public bool IsDisposed => _isDisposed;

    #endregion Public 属性

    #region Protected 方法

    /// <summary>
    /// 减少引用
    /// </summary>
    protected void DecrementRef()
    {
        ThrowIfDisposed();

        int count;
        bool doDispose;

        lock (SyncRoot)
        {
            count = _count--;
            doDispose = count == 0 && _prepareDrop;
        }

        if (count < 0)
        {
            throw new InvalidOperationException("reference can not less than 0.");
        }

        if (doDispose)
        {
            Dispose();
        }
    }

    /// <summary>
    /// 增加引用
    /// </summary>
    protected void IncrementRef()
    {
        ThrowIfDisposed();
        ThrowIfPreparedDrop();

        lock (SyncRoot)
        {
            ThrowIfPreparedDrop();
            _count++;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    /// <summary>
    ///
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    protected void ThrowIfPreparedDrop()
    {
        if (_prepareDrop)
        {
            throw new InvalidOperationException("object is prepared drop.");
        }
    }

    #endregion Protected 方法

    #region Public 方法

    /// <inheritdoc/>
    public abstract Task HoldAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 准备放弃，不允许后续引用
    /// </summary>
    public void PrepareDrop()
    {
        bool doDispose;
        lock (SyncRoot)
        {
            _prepareDrop = true;
            doDispose = _count == 0;
        }

        if (doDispose)
        {
            Dispose();
        }
    }

    /// <inheritdoc/>
    public abstract void Release();

    #endregion Public 方法

    #region Dispose

    /// <summary>
    ///
    /// </summary>
    ~JuxtaposeExecutorHolder()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc cref="Dispose()"/>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            Executor.Dispose();
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
