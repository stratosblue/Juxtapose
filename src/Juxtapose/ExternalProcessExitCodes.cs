namespace Juxtapose;

/// <summary>
/// 外部进程退出码
/// </summary>
public enum ExternalProcessExitCodes : byte //Linux下控制在255以内，避免接收不正常
{
    /// <summary>
    /// 父进程退出
    /// </summary>
    ParentProcessExited = 0xD0, //208

    /// <summary>
    /// 没有找到初始化上下文
    /// </summary>
    InitializationContextNotFound,

    /// <summary>
    /// 启动选项必要字段缺失
    /// </summary>
    StartupOptionsRequiredFieldMissing,

    /// <summary>
    /// 查找父进程失败
    /// </summary>
    FindParentProcessFail,

    /// <summary>
    /// Juxtapose版本号不匹配
    /// </summary>
    JuxtaposeVersionNotMatch,

    /// <summary>
    /// 没有 Juxtapose 启动命令行参数
    /// </summary>
    NoJuxtaposeCommandLineArguments,
}