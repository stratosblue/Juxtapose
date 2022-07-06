using System;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose;

/// <summary>
/// <see cref="JuxtaposeExecutor"/>池
/// </summary>
public interface IJuxtaposeExecutorPool : IDisposable
{
    #region Public 方法

    /// <summary>
    /// 获取执行器
    /// </summary>
    /// <param name="creationContext"></param>
    /// <param name="cancellation">取消Token</param>
    /// <returns></returns>
    Task<IJuxtaposeExecutorOwner> GetAsync(ExecutorCreationContext creationContext, CancellationToken cancellation);

    #endregion Public 方法
}