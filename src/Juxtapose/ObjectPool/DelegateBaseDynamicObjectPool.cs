namespace Juxtapose.ObjectPool;

internal class DelegateBaseDynamicObjectPool<T> : DynamicObjectPool<T>
{
    #region Private 字段

    private readonly Func<CancellationToken, Task<T>> _createDelegate;

    private readonly Action<T> _destroyDelegate;

    private readonly ResourcePressureDelegate _resourcePressureDelegate;

    #endregion Private 字段

    #region Public 构造函数

    public DelegateBaseDynamicObjectPool(IDynamicObjectPoolScheduler<T> scheduler,
                                         Func<CancellationToken, Task<T>> createDelegate,
                                         Action<T> destroyDelegate,
                                         ResourcePressureDelegate resourcePressureDelegate)
        : base(scheduler)
    {
        _createDelegate = createDelegate ?? throw new ArgumentNullException(nameof(createDelegate));
        _destroyDelegate = destroyDelegate ?? throw new ArgumentNullException(nameof(destroyDelegate));
        _resourcePressureDelegate = resourcePressureDelegate ?? throw new ArgumentNullException(nameof(resourcePressureDelegate));
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override Task<T> CreateAsync(CancellationToken cancellation = default) => _createDelegate(cancellation);

    protected override void DoDestroy(T instance) => _destroyDelegate(instance);

    protected override void OnSchedulerResourcePressure(ResourcePressureLevel level) => _resourcePressureDelegate(level);

    #endregion Protected 方法
}
