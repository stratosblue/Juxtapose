using System.Runtime.Serialization;

namespace Juxtapose;

/// <summary>
/// 外部进程已退出异常
/// </summary>
[Serializable]
public class ExternalProcessExitedException : JuxtaposeException
{
    #region Public 属性

    /// <summary>
    /// 进程ExitCode
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// 进程ID
    /// </summary>
    public int ProcessId { get; }

    #endregion Public 属性

    #region Protected 构造函数

    /// <inheritdoc cref="ExternalProcessExitedException"/>
    protected ExternalProcessExitedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    #endregion Protected 构造函数

    #region Public 构造函数

    /// <inheritdoc cref="ExternalProcessExitedException"/>
    public ExternalProcessExitedException(int processId, int exitCode, string? message) : base(message)
    {
        ProcessId = processId;
        ExitCode = exitCode;
    }

    /// <inheritdoc cref="ExternalProcessExitedException"/>
    public ExternalProcessExitedException(int processId, int exitCode, string? message, Exception? innerException) : base(message, innerException)
    {
        ProcessId = processId;
        ExitCode = exitCode;
    }

    #endregion Public 构造函数
}
