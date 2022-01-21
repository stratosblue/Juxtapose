using System;
using System.Diagnostics;

using Juxtapose.Utils;

namespace Juxtapose
{
    /// <summary>
    /// 本地<inheritdoc cref="IExternalProcessActivator"/>
    /// </summary>
    public class LocalExternalProcessActivator : IExternalProcessActivator
    {
        #region Private 字段

        private readonly LocalExternalProcessActivatorOptions _options;

        private readonly ProcessStartInfo _referenceStartInfo;

        #endregion Private 字段

        #region Public 构造函数

        /// <inheritdoc cref="LocalExternalProcessActivator"/>
        public LocalExternalProcessActivator(LocalExternalProcessActivatorOptions? options = null)
        {
            _options = options ?? new();
            _referenceStartInfo = _options.ReferenceStartInfo ?? LocalExternalProcessActivatorOptions.CreateDefaultReferenceProcessStartInfo();
        }

        #endregion Public 构造函数

        #region Protected 方法

        /// <summary>
        /// 创建进程启动信息
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        protected virtual ProcessStartInfo CreateProcessStartInfo(IJuxtaposeOptions options)
        {
            var processStartInfo = _referenceStartInfo.Clone();

            options.SessionId = Guid.NewGuid().ToString("N");

            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            var enableDebugger = isWindows && Debugger.IsAttached;

            options.EnableDebugger = enableDebugger;

            ExternalProcessArgumentUtil.SetAsJuxtaposeProcessStartInfo(processStartInfo, options);

            if (!enableDebugger)
            {
                //https://docs.microsoft.com/zh-cn/dotnet/core/run-time-config/debugging-profiling

                //HACK 默认关闭子进程所有诊断信息
                //HACK 在不支持.net6之前的版本时，修改环境变量 COMPlus_ 为 DOTNET_
                processStartInfo.EnvironmentVariables["DOTNET_USE_POLLING_FILE_WATCHER"] = "0";
                processStartInfo.EnvironmentVariables["COMPlus_EnableDiagnostics"] = "0";
                processStartInfo.EnvironmentVariables["CORECLR_ENABLE_PROFILING"] = "0";
                processStartInfo.EnvironmentVariables["COMPlus_PerfMapEnabled"] = "0";
                processStartInfo.EnvironmentVariables["COMPlus_PerfMapIgnoreSignal"] = "0";
            }

            return processStartInfo;
        }

        #endregion Protected 方法

        #region Public 方法

        /// <summary>
        /// 快速创建参照进程启动信息（非绝对路径时，内部会尝试自动在当前目录下查找文件）
        /// </summary>
        /// <param name="entryName">入口程序名称</param>
        /// <param name="args">启动参数</param>
        /// <returns></returns>
        public static LocalExternalProcessActivator FastCreate(string entryName, params string[] args)
        {
            return new(new()
            {
                ReferenceStartInfo = LocalExternalProcessActivatorOptions.FastCreateProcessStartInfo(entryName, args)
            });
        }

        /// <inheritdoc/>
        public virtual IExternalProcess CreateProcess(IJuxtaposeOptions options)
        {
            var processStartInfo = CreateProcessStartInfo(options);
            return new LocalExternalProcess(processStartInfo);
        }

        /// <inheritdoc/>
        public void Dispose()
        { }

        #endregion Public 方法
    }
}