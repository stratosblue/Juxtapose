namespace Juxtapose.ObjectPool;

/// <summary>
/// 资源压力等级
/// </summary>
public enum ResourcePressureLevel
{
    /// <summary>
    /// 默认
    /// </summary>
    Default = 0,

    /// <summary>
    /// 低
    /// </summary>
    Low = 1,

    /// <summary>
    /// 普通
    /// </summary>
    Normal = 2,

    /// <summary>
    /// 高
    /// </summary>
    High = 3,
}
