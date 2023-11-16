namespace Juxtapose;

/// <summary>
/// Juxtapose环境
/// </summary>
public static class JuxtaposeEnvironment
{
    #region Public 属性

    /// <summary>
    /// 当前进程是否是子进程<para/>
    /// 仅当使用<see cref="JuxtaposeEntryPoint"/>类设置启动点时，此属性值才会正常设置
    /// </summary>
    public static bool IsSubProcess { get; internal set; } = false;

    #endregion Public 属性
}
