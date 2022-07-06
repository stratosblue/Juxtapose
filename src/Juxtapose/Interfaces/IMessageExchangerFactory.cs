using System;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose;

/// <summary>
/// 消息交换机工厂
/// </summary>
public interface IMessageExchangerFactory : IDisposable
{
    #region Public 方法

    /// <summary>
    /// 创建 <see cref="IMessageExchanger"/>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<IMessageExchanger> CreateAsync(IJuxtaposeOptions options, CancellationToken cancellation = default);

    /// <summary>
    /// 创建主机端的 <see cref="IMessageExchanger"/>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task<IMessageExchanger> CreateHostAsync(IJuxtaposeOptions options, CancellationToken cancellation = default);

    #endregion Public 方法
}