using System.Diagnostics;

namespace Juxtapose;

/// <summary>
///
/// </summary>
public static class ProcessStartInfoExtensions
{
    #region Public 方法

    /// <summary>
    /// 复制一个新的<see cref="ProcessStartInfo"/>
    /// </summary>
    /// <returns></returns>
    public static ProcessStartInfo Clone(this ProcessStartInfo processStartInfo)
    {
        var newProcessInfo = new ProcessStartInfo()
        {
            //ArgumentList = processStartInfo.ArgumentList,     copy by item
            Arguments = processStartInfo.Arguments,
            CreateNoWindow = processStartInfo.CreateNoWindow,
            //Domain = processStartInfo.Domain,     ignore
            //Environment = processStartInfo.Environment,       copy by item
            //EnvironmentVariables = processStartInfo.EnvironmentVariables,     ignore
            ErrorDialog = processStartInfo.ErrorDialog,
            ErrorDialogParentHandle = processStartInfo.ErrorDialogParentHandle,
            FileName = processStartInfo.FileName,
            //LoadUserProfile = processStartInfo.LoadUserProfile,           ignore
            //Password = processStartInfo.Password,                         ignore
            //PasswordInClearText = processStartInfo.PasswordInClearText,   ignore
            RedirectStandardError = processStartInfo.RedirectStandardError,
            RedirectStandardInput = processStartInfo.RedirectStandardInput,
            RedirectStandardOutput = processStartInfo.RedirectStandardOutput,
            StandardErrorEncoding = processStartInfo.StandardErrorEncoding,
            StandardInputEncoding = processStartInfo.StandardInputEncoding,
            StandardOutputEncoding = processStartInfo.StandardOutputEncoding,
            UserName = processStartInfo.UserName,
            UseShellExecute = processStartInfo.UseShellExecute,
            Verb = processStartInfo.Verb,
            //Verbs = processStartInfo.Verbs,       ignore
            WindowStyle = processStartInfo.WindowStyle,
            WorkingDirectory = processStartInfo.WorkingDirectory,
        };

        foreach (var item in processStartInfo.ArgumentList)
        {
            newProcessInfo.ArgumentList.Add(item);
        }

        foreach (var item in processStartInfo.Environment)
        {
            newProcessInfo.Environment.Add(item);
        }

        return newProcessInfo;
    }

    #endregion Public 方法
}