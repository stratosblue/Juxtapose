namespace Juxtapose;

/// <summary>
/// 无持有限制的 <see cref="JuxtaposeExecutor"/> 持有器
/// </summary>
public sealed class NoLimitJuxtaposeExecutorHolder(JuxtaposeExecutor executor)
    : JuxtaposeExecutorHolder(executor)
{
    #region Private 字段

    private readonly Task<JuxtaposeExecutor> _holdResult = Task.FromResult(executor);

    #endregion Private 字段

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
