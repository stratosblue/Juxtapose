using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Juxtapose
{
    /// <summary>
    /// 本地外部进程激活器选项
    /// </summary>
    public class LocalExternalProcessActivatorOptions
    {
        #region Private 字段

        private static string[]? s_commandLineArgs;
        private static string? s_mainModuleFileName;

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 用于参照的 <see cref="ProcessStartInfo"/>（启动新进程时，参照此对象设置启动信息）
        /// </summary>
        public ProcessStartInfo? ReferenceStartInfo { get; set; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="LocalExternalProcessActivatorOptions"/>
        public LocalExternalProcessActivatorOptions(ProcessStartInfo? referenceStartInfo = null)
        {
            ReferenceStartInfo = referenceStartInfo;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <summary>
        /// 创建默认的参照进程启动信息
        /// </summary>
        /// <returns></returns>
        public static ProcessStartInfo CreateDefaultReferenceProcessStartInfo()
        {
            var fileName = s_mainModuleFileName ??= Process.GetCurrentProcess().MainModule?.FileName;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new JuxtaposeException("Can not get current process start file. Please try with manual.");
            }

            var commandLineArgs = s_commandLineArgs ??= Environment.GetCommandLineArgs();

            if (commandLineArgs.Length > 0)
            {
                //去除不必要的启动参数
                //HACK 启动参数在复杂情况下可能有问题，但目前好像运行良好，先这样吧
                var fileNameInCommandLine = Path.GetFileName(commandLineArgs[0]);
                if (fileNameInCommandLine == Path.GetFileName(fileName))
                {
                    commandLineArgs = commandLineArgs.Skip(1).ToArray();
                }
            }

            return CreateProcessStartInfo(fileName, commandLineArgs);
        }

        /// <summary>
        /// 快速创建参照进程启动信息（非绝对路径时，内部会尝试自动在当前目录下查找文件）
        /// </summary>
        /// <param name="entryName">入口程序名称</param>
        /// <param name="args">启动参数</param>
        /// <returns></returns>
        public static ProcessStartInfo FastCreateProcessStartInfo(string entryName, params string[] args)
        {
            GetSubProcessEntryFilePath(entryName, out var executableFile, out var dllFile);

            string fileName;
            if (string.IsNullOrWhiteSpace(executableFile)
                && !string.IsNullOrWhiteSpace(dllFile))
            {
                fileName = "dotnet";
                args = new[] { dllFile }.Concat(args).ToArray();
            }
            else if (!string.IsNullOrWhiteSpace(executableFile))
            {
                fileName = executableFile;
            }
            else
            {
                throw new JuxtaposeException("Can not get current process start file. Please try with manual.");
            }

            return CreateProcessStartInfo(fileName, args);
        }

        #endregion Public 方法

        #region Private 方法

        private static ProcessStartInfo CreateProcessStartInfo(string fileName, string[] commandLineArgs)
        {
            var processStartInfo = new ProcessStartInfo(fileName)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory,
            };

            foreach (var item in commandLineArgs)
            {
                processStartInfo.ArgumentList.Add(item);
            }

            return processStartInfo;
        }

        private static void GetSubProcessEntryFilePath(string entryFilePath, out string? executableFile, out string? dllFile)
        {
            if (!Path.IsPathRooted(entryFilePath))
            {
                entryFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, entryFilePath);
            }

            executableFile = entryFilePath;
            dllFile = null;
            if (executableFile.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                dllFile = executableFile;
                executableFile = executableFile[0..^4];
            }

            if (OperatingSystem.IsWindows())
            {
                if (!executableFile.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    executableFile = $"{executableFile}.exe";
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                if (executableFile.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    executableFile = executableFile[0..^4];
                }
            }
            else
            {
                //HACK Other Platform
            }

            if (!File.Exists(executableFile))
            {
                executableFile = null;
            }
        }

        #endregion Private 方法
    }
}