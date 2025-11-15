namespace Juxtapose;

/// <inheritdoc cref="IJuxtaposeExecutorOwner"/>
public sealed class JuxtaposeExecutorOwner(string identifier,
                                           IJuxtaposeExecutorHolder executorHolder,
                                           ExecutorCreationContext creationContext,
                                           IExecutorPoolPolicy executorPoolPolicy,
                                           ExecutorHolderDestroyCallback destroyCallback)
    : IJuxtaposeExecutorOwner
{
    #region Private 字段

    private readonly ExecutorCreationContext _creationContext = creationContext ?? throw new ArgumentNullException(nameof(creationContext));

    private readonly ExecutorHolderDestroyCallback _destroyCallback = destroyCallback ?? throw new ArgumentNullException(nameof(destroyCallback));

    private readonly IJuxtaposeExecutorHolder _executorHolder = executorHolder ?? throw new ArgumentNullException(nameof(executorHolder));

    private readonly IExecutorPoolPolicy _executorPoolPolicy = executorPoolPolicy ?? throw new ArgumentNullException(nameof(executorPoolPolicy));

    private readonly string _identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));

    private int _isDisposed = 0;

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public JuxtaposeExecutor Executor
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed > 0, this);
            return _executorHolder.Executor;
        }
    }

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Increment(ref _isDisposed) == 1)
        {
            _executorHolder.Release();

            if (_executorPoolPolicy.ShouldDropExecutor(_creationContext, _executorHolder))
            {
                _destroyCallback(_identifier);
            }
        }
    }

    #endregion Public 方法
}
