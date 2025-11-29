using System.Diagnostics;

using Juxtapose.Utils;

namespace Juxtapose;

/// <summary>
/// 本地<inheritdoc cref="IExternalProcessActivator"/>
/// </summary>
public class LocalExternalProcessActivator : IExternalProcessActivator
{
    #region Public 属性

    /// <summary>
    /// 启用外部进程的.net诊断
    /// </summary>
    /// <remarks>
    /// https://learn.microsoft.com/zh-cn/dotnet/core/runtime-config/debugging-profiling
    /// </remarks>
    public static bool EnableDotnetDiagnostics { get; set; } = false;

    #endregion Public 属性

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

        const string EnvDOTNET_EnableDiagnostics = "DOTNET_EnableDiagnostics";

        if (Environment.GetEnvironmentVariable(EnvDOTNET_EnableDiagnostics) == null)
        {
            //https://learn.microsoft.com/zh-cn/dotnet/core/runtime-config/debugging-profiling
            //https://learn.microsoft.com/zh-cn/dotnet/core/tools/dotnet-environment-variables#dotnet_enablediagnostics

            //HACK 非调试模式并且没有显式指定诊断状态，默认关闭子进程的诊断（避免生成过多诊断支持文件）
            processStartInfo.EnvironmentVariables[EnvDOTNET_EnableDiagnostics] = "0";
        }

        if (EnableDotnetDiagnostics)    //强制启用
        {
            processStartInfo.EnvironmentVariables[EnvDOTNET_EnableDiagnostics] = "1";
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

    #endregion Public 方法

    #region IDisposable

    /// <summary>
    ///
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}
