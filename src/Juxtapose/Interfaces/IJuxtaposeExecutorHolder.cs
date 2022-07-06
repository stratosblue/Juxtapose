using System;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose;

/// <summary>
/// <see cref="JuxtaposeExecutor"/> 持有器
/// </summary>
public interface IJuxtaposeExecutorHolder : IDisposable
{
    #region Public 属性

    /// <summary>
    /// 当前已持有数
    /// </summary>
    int Count { get; }

    /// <summary>
    /// <see cref="JuxtaposeExecutor"/>
    /// </summary>
    JuxtaposeExecutor Executor { get; }

    /// <summary>
    /// 是否已释放
    /// </summary>
    bool IsDisposed { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 持有
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HoldAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 准备放弃，不允许后续引用
    /// </summary>
    void PrepareDrop();

    /// <summary>
    /// 释放持有
    /// </summary>
    void Release();

    #endregion Public 方法
}