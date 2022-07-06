using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose;

/// <summary>
/// 消息交换机
/// </summary>
public interface IMessageExchanger : IInitializationable, IDisposable
{
    #region Public 事件

    /// <summary>
    /// 无效时触发的事件
    /// </summary>
    event Action<object>? OnInvalid;

    #endregion Public 事件

    #region Public 方法

    /// <summary>
    /// 获取消息
    /// </summary>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    IAsyncEnumerable<object> GetMessagesAsync(CancellationToken cancellation);

    /// <summary>
    /// 写入消息
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task WriteMessageAsync(object message, CancellationToken cancellation);

    #endregion Public 方法
}