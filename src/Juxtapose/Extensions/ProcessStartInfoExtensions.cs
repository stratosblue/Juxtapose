using System.Diagnostics;

namespace Juxtapose
{
    /// <summary>
    ///
    /// </summary>
    public static class ProcessStartInfoExtensions
    {
        #region Public 方法

        /// <summary>
        /// 序列化为字符串
        /// </summary>
        /// <returns></returns>
        public static ProcessStartInfo Clone(this ProcessStartInfo processStartInfo)
        {
            return new ProcessStartInfo()
            {
                //ArgumentList = processStartInfo.ArgumentList,
                Arguments = processStartInfo.Arguments,
                CreateNoWindow = processStartInfo.CreateNoWindow,
                //Domain = processStartInfo.Domain,
                //Environment = processStartInfo.Environment,
                //EnvironmentVariables = processStartInfo.EnvironmentVariables,
                ErrorDialog = processStartInfo.ErrorDialog,
                ErrorDialogParentHandle = processStartInfo.ErrorDialogParentHandle,
                FileName = processStartInfo.FileName,
                //LoadUserProfile = processStartInfo.LoadUserProfile,
                //Password = processStartInfo.Password,
                //PasswordInClearText = processStartInfo.PasswordInClearText,
                RedirectStandardError = processStartInfo.RedirectStandardError,
                RedirectStandardInput = processStartInfo.RedirectStandardInput,
                RedirectStandardOutput = processStartInfo.RedirectStandardOutput,
                StandardErrorEncoding = processStartInfo.StandardErrorEncoding,
                StandardInputEncoding = processStartInfo.StandardInputEncoding,
                StandardOutputEncoding = processStartInfo.StandardOutputEncoding,
                UserName = processStartInfo.UserName,
                UseShellExecute = processStartInfo.UseShellExecute,
                Verb = processStartInfo.Verb,
                //Verbs = processStartInfo.Verbs,
                WindowStyle = processStartInfo.WindowStyle,
                WorkingDirectory = processStartInfo.WorkingDirectory,
            };
        }

        #endregion Public 方法
    }
}