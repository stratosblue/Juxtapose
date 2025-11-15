namespace Juxtapose;

/// <summary>
/// 允许有限持有的 <see cref="JuxtaposeExecutor"/> 持有器
/// </summary>
public sealed class LimitedJuxtaposeExecutorHolder : JuxtaposeExecutorHolder
{
    #region Private 字段

    private readonly SemaphoreSlim _semaphore;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="LimitedJuxtaposeExecutorHolder"/>
    public LimitedJuxtaposeExecutorHolder(JuxtaposeExecutor executor, int limit) : base(executor)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        _semaphore = new(limit, limit);
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override async Task HoldAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ThrowIfPreparedDrop();
        IncrementRef();
        await _semaphore.WaitAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override void Release()
    {
        DecrementRef();
        _semaphore.Release();
    }

    #endregion Public 方法

    #region Protected 方法

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _semaphore.Dispose();
        base.Dispose(disposing);
    }

    #endregion Protected 方法
}
