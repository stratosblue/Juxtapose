namespace Juxtapose;

/// <summary>
/// 幻象类接口
/// </summary>
public interface IIllusion : IDisposable
{
    #region Public 属性

    /// <summary>
    /// 绑定的执行器
    /// </summary>
    JuxtaposeExecutor Executor { get; }

    /// <summary>
    /// 是否可用
    /// </summary>
    bool IsAvailable { get; }

    #endregion Public 属性
}
