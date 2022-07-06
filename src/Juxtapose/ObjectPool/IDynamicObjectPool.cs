using System;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool;

/// <summary>
/// 动态的对象池
/// </summary>
public interface IDynamicObjectPool<T> : IDisposable
{
    #region Public 方法

    /// <summary>
    /// 尝试借用一个对象（没有可用对象时返回 null）
    /// </summary>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<T?> RentAsync(CancellationToken cancellation = default);

    /// <summary>
    /// 归还借用的对象
    /// </summary>
    /// <param name="instance"></param>
    void Return(T? instance);

    #endregion Public 方法
}