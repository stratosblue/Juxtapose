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

            ExternalProcessArgumentUtil.SetAsJuxtaposeProcessStartInfo(processStartInfo, options);

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