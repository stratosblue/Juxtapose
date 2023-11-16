using Juxtapose.Messages;

namespace Juxtapose;

/// <summary>
/// <see cref="JuxtaposeMessage"/> 执行器
/// </summary>
public interface IMessageExecutor
{
    #region Public 方法

    /// <summary>
    /// 执行消息
    /// </summary>
    /// <param name="executor"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<JuxtaposeMessage?> ExecuteAsync(JuxtaposeExecutor executor, JuxtaposeMessage message);

    #endregion Public 方法
}
