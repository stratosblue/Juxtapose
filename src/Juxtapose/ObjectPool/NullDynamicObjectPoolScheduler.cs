using System;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool;

/// <summary>
/// 空的<inheritdoc cref="IDynamicObjectPoolScheduler{T}"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public class NullDynamicObjectPoolScheduler<T>
    : IDynamicObjectPoolScheduler<T>
{
    #region Public 事件

    /// <inheritdoc/>
    public event ResourcePressureDelegate? OnResourcePressure
    {
        add { }
        remove { }
    }

    #endregion Public 事件

    #region Public 属性

    /// <summary>
    /// 公共实例
    /// </summary>
    public static IDynamicObjectPoolScheduler<T> Instance { get; } = new NullDynamicObjectPoolScheduler<T>();

    #endregion Public 属性

    #region Private 构造函数

    private NullDynamicObjectPoolScheduler()
    {
    }

    #endregion Private 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public ValueTask<bool> CanCreateAsync(CancellationToken cancellation = default)
    {
        return ValueTask.FromResult(false);
    }

    /// <inheritdoc/>
    public Task LockAsync(CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void OnCreated(T instance)
    {
    }

    /// <inheritdoc/>
    public void OnDestroyed(T instance)
    {
    }

    /// <inheritdoc/>
    public bool OnRent(T instance) => false;

    /// <inheritdoc/>
    public bool OnReturn(T instance) => false;

    /// <inheritdoc/>
    public void ReleaseLock()
    {
    }

    #endregion Public 方法

    #region IDisposable

    /// <summary>
    ///
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}