namespace Juxtapose;

/// <summary>
/// 无持有限制的 <see cref="JuxtaposeExecutor"/> 持有器
/// </summary>
public sealed class NoLimitJuxtaposeExecutorHolder : JuxtaposeExecutorHolder
{
    #region Private 字段

    private readonly Task<JuxtaposeExecutor> _holdResult;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="NoLimitJuxtaposeExecutorHolder"/>
    public NoLimitJuxtaposeExecutorHolder(JuxtaposeExecutor executor) : base(executor)
    {
        _holdResult = Task.FromResult(executor);
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override Task HoldAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ThrowIfPreparedDrop();
        IncrementRef();
        return _holdResult;
    }

    /// <inheritdoc/>
    public override void Release()
    {
        DecrementRef();
    }

    #endregion Public 方法
}
