namespace Juxtapose.Utils;

/// <summary>
/// <see cref="ExternalProcessExitCodes"/> 工具
/// </summary>
public static class ExternalProcessExitCodeUtil
{
    #region Public 方法

    /// <summary>
    /// 获取进程退出码的描述
    /// </summary>
    /// <param name="exitCode"></param>
    /// <returns></returns>
    public static string GetExitCodeDescription(int exitCode)
    {
        var code = (ExternalProcessExitCodes)exitCode;
        return code switch
        {
            ExternalProcessExitCodes.ParentProcessExited => $"ExitCode {exitCode} - {nameof(ExternalProcessExitCodes.ParentProcessExited)}",
            ExternalProcessExitCodes.InitializationContextNotFound => $"ExitCode {exitCode} - {nameof(ExternalProcessExitCodes.InitializationContextNotFound)}",
            ExternalProcessExitCodes.StartupOptionsRequiredFieldMissing => $"ExitCode {exitCode} - {nameof(ExternalProcessExitCodes.StartupOptionsRequiredFieldMissing)}",
            ExternalProcessExitCodes.FindParentProcessFail => $"ExitCode {exitCode} - {nameof(ExternalProcessExitCodes.FindParentProcessFail)}",
            ExternalProcessExitCodes.JuxtaposeVersionNotMatch => $"ExitCode {exitCode} - {nameof(ExternalProcessExitCodes.JuxtaposeVersionNotMatch)}",
            ExternalProcessExitCodes.NoJuxtaposeCommandLineArguments => $"ExitCode {exitCode} - {nameof(ExternalProcessExitCodes.NoJuxtaposeCommandLineArguments)}",
            _ => $"Unknown exit code {exitCode}",
        };
    }

    #endregion Public 方法
}
