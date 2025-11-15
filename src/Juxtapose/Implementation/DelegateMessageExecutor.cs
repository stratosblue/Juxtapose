using Juxtapose.Messages;

namespace Juxtapose;

/// <summary>
/// 基于委托的消息执行器
/// </summary>
public class AsyncDelegateMessageExecutor(Func<JuxtaposeExecutor, JuxtaposeMessage, Task<JuxtaposeMessage?>?> callback)
    : DelegateMessageExecutor(callback)
{
}

/// <summary>
/// 基于委托的消息执行器
/// </summary>
public class DelegateMessageExecutor(Func<JuxtaposeExecutor, JuxtaposeMessage, Task<JuxtaposeMessage?>?> callback)
    : IMessageExecutor
{
    #region Private 字段

    private readonly Func<JuxtaposeExecutor, JuxtaposeMessage, Task<JuxtaposeMessage?>?> _callback = callback ?? throw new ArgumentNullException(nameof(callback));

    #endregion Private 字段

    #region Public 方法

    /// <inheritdoc/>
    public Task<JuxtaposeMessage?> ExecuteAsync(JuxtaposeExecutor executor, JuxtaposeMessage message)
    {
        return _callback(executor, message) ?? Task.FromResult<JuxtaposeMessage?>(null);
    }

    #endregion Public 方法
}

/// <summary>
/// 基于委托的消息执行器
/// </summary>
public class SyncDelegateMessageExecutor(Func<JuxtaposeExecutor, JuxtaposeMessage, JuxtaposeMessage?> callback)
    : DelegateMessageExecutor(CreateAsyncCallback(callback))
{
    #region Private 方法

    private static Func<JuxtaposeExecutor, JuxtaposeMessage, Task<JuxtaposeMessage?>> CreateAsyncCallback(Func<JuxtaposeExecutor, JuxtaposeMessage, JuxtaposeMessage?> callback)
    {
        return callback is null
               ? throw new ArgumentNullException(nameof(callback))
               : (executor, message) => Task.FromResult(callback(executor, message));
    }

    #endregion Private 方法
}
