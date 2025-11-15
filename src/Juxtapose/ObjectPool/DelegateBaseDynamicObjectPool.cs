namespace Juxtapose.ObjectPool;

internal class DelegateBaseDynamicObjectPool<T>(IDynamicObjectPoolScheduler<T> scheduler,
                                                Func<CancellationToken, Task<T>> createDelegate,
                                                Action<T> destroyDelegate,
                                                ResourcePressureDelegate resourcePressureDelegate)
    : DynamicObjectPool<T>(scheduler)
{
    #region Private 字段

    private readonly Func<CancellationToken, Task<T>> _createDelegate = createDelegate ?? throw new ArgumentNullException(nameof(createDelegate));

    private readonly Action<T> _destroyDelegate = destroyDelegate ?? throw new ArgumentNullException(nameof(destroyDelegate));

    private readonly ResourcePressureDelegate _resourcePressureDelegate = resourcePressureDelegate ?? throw new ArgumentNullException(nameof(resourcePressureDelegate));

    #endregion Private 字段

    #region Protected 方法

    protected override Task<T> CreateAsync(CancellationToken cancellation = default) => _createDelegate(cancellation);

    protected override void DoDestroy(T instance) => _destroyDelegate(instance);

    protected override void OnSchedulerResourcePressure(ResourcePressureLevel level) => _resourcePressureDelegate(level);

    #endregion Protected 方法
}
