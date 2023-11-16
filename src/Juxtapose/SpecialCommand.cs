namespace Juxtapose;

/// <summary>
/// 专用命令
/// 0 - 255
/// </summary>
#if JUXTAPOSE_SOURCE_GENERATOR

internal enum SpecialCommand : byte

#else

public enum SpecialCommand : byte

#endif
{
    /// <summary>
    /// 未定义
    /// </summary>
    Undefined,

    /// <summary>
    /// 从 <see cref="IServiceProvider"/> 获取实例
    /// </summary>
    GetInstanceByServiceProvider = 20,

    /// <summary>
    /// 取消 <see cref="CancellationToken"/>
    /// </summary>
    CancelCancellationToken = 50,
}
