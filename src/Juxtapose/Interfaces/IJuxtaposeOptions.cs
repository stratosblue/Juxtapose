namespace Juxtapose;

/// <summary>
/// Juxtapose选项
/// </summary>
public interface IJuxtaposeOptions : ISettingCollection, IReadOnlySettingCollection
{
    #region Public 属性

    /// <summary>
    /// <see cref="IInitializationContext"/> 标识符
    /// </summary>
    string ContextIdentifier { get; set; }

    /// <summary>
    /// 启用Debugger
    /// </summary>
    bool EnableDebugger { get; set; }

    /// <summary>
    /// 父进程ID
    /// </summary>
    int? ParentProcessId { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    string SessionId { get; set; }

    /// <summary>
    /// 版本号，用以子进程兼容性检查
    /// </summary>
    uint Version { get; set; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 克隆
    /// </summary>
    /// <returns></returns>
    IJuxtaposeOptions Clone();

    #endregion Public 方法
}
