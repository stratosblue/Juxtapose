using Juxtapose.Messages;

namespace Juxtapose;

/// <summary>
/// 基于委托的消息执行器
/// </summary>
public class AsyncDelegateMessageExecutor : DelegateMessageExecutor
{
    #region Public 构造函数

    /// <inheritdoc cref="AsyncDelegateMessageExecutor"/>
    public AsyncDelegateMessageExecutor(Func<JuxtaposeExecutor, JuxtaposeMessage, Task<JuxtaposeMessage?>?> callback) : base(callback)
    {
    }

    #endregion Public 构造函数
}

/// <summary>
/// 基于委托的消息执行器
/// </summary>
public class DelegateMessageExecutor : IMessageExecutor
{
    #region Private 字段

    private readonly Func<JuxtaposeExecutor, JuxtaposeMessage, Task<JuxtaposeMessage?>?> _callback;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="DelegateMessageExecutor"/>
    public DelegateMessageExecutor(Func<JuxtaposeExecutor, JuxtaposeMessage, Task<JuxtaposeMessage?>?> callback)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    #endregion Public 构造函数

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
public class SyncDelegateMessageExecutor : DelegateMessageExecutor
{
    #region Public 构造函数

    /// <inheritdoc cref="SyncDelegateMessageExecutor"/>
    public SyncDelegateMessageExecutor(Func<JuxtaposeExecutor, JuxtaposeMessage, JuxtaposeMessage?> callback) : base(CreateAsyncCallback(callback))
    {
    }

    #endregion Public 构造函数

    #region Private 方法

    private static Func<JuxtaposeExecutor, JuxtaposeMessage, Task<JuxtaposeMessage?>> CreateAsyncCallback(Func<JuxtaposeExecutor, JuxtaposeMessage, JuxtaposeMessage?> callback)
    {
        return callback is null ? throw new ArgumentNullException(nameof(callback))
                                : (executor, message) => Task.FromResult(callback(executor, message));
    }

    #endregion Private 方法
}
