namespace Juxtapose
{
    /// <summary>
    /// 外部进程退出码
    /// </summary>
    public enum ExternalProcessExitCodes
    {
        /// <summary>
        /// 父进程退出
        /// </summary>
        ParentProcessExited = -1000000000,

        /// <summary>
        /// 没有找到初始化上下文
        /// </summary>
        InitializationContextNotFound = -1000000001,

        /// <summary>
        /// 启动选项必要字段缺失
        /// </summary>
        StartupOptionsRequiredFieldMissing = -1000000002,

        /// <summary>
        /// 查找父进程失败
        /// </summary>
        FindParentProcessFail = -1000000003,

        /// <summary>
        /// Juxtapose版本号不匹配
        /// </summary>
        JuxtaposeVersionNotMatch = -1000000004,

        /// <summary>
        /// 没有 Juxtapose 启动命令行参数
        /// </summary>
        NoJuxtaposeCommandLineArguments = -1000000005,
    }
}