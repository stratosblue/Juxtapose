using System;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool;

/// <summary>
/// 动态对象池规划器
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDynamicObjectPoolScheduler<in T> : IDisposable
{
    #region Public 事件

    /// <summary>
    /// 计划程序资源紧张事件（池应当在此事件触发时进行资源释放）
    /// </summary>
    event ResourcePressureDelegate? OnResourcePressure;

    #endregion Public 事件

    #region Public 方法

    #region Create

    /// <summary>
    /// 检查当前是否可以创建新的对象
    /// </summary>
    /// <param name="cancellation"></param>
    /// <returns>返回true可以创建，false不可创建</returns>
    ValueTask<bool> CanCreateAsync(CancellationToken cancellation = default);

    /// <summary>
    /// 获取锁
    /// </summary>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task LockAsync(CancellationToken cancellation);

    /// <summary>
    /// 释放锁
    /// </summary>
    void ReleaseLock();

    #endregion Create

    /// <summary>
    /// 在对象创建时
    /// </summary>
    /// <param name="instance"></param>
    void OnCreated(T instance);

    /// <summary>
    /// 在对象销毁时
    /// </summary>
    /// <param name="instance"></param>
    void OnDestroyed(T instance);

    /// <summary>
    /// 在对象借用时
    /// </summary>
    /// <param name="instance"></param>
    /// <returns>返回false，则取消<paramref name="instance"/>的借用</returns>
    bool OnRent(T instance);

    /// <summary>
    /// 在对象归还时
    /// </summary>
    /// <param name="instance"></param>
    /// <returns>返回false，则<paramref name="instance"/>不应再添加到池中</returns>
    bool OnReturn(T instance);

    #endregion Public 方法
}