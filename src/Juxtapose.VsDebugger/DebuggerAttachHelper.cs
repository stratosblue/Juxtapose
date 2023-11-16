using System.Reflection;
using System.Runtime.Versioning;

namespace System;

/// <summary>
/// Debugger附加帮助器
/// </summary>
public static class DebuggerAttachHelper
{
    #region Public 方法

    /// <summary>
    /// 附加到指定进程的Debugger
    /// </summary>
    /// <param name="targetProcessId"></param>
    [SupportedOSPlatform("windows")]
    public static void AttachTo(int targetProcessId)
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
        try
        {
            VsDebuggerAttacher.AttachToTargetProcessDebugger(Environment.ProcessId, targetProcessId);
        }
        finally
        {
            AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolve;
        }
    }

    #endregion Public 方法

    #region Private 方法

    private static Assembly? AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        try
        {
            if (args.RequestingAssembly == Assembly.GetExecutingAssembly())
            {
                var dllName = $"{args.Name.Split(',')[0]}.dll";
                var tryLoadDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName);
                if (File.Exists(tryLoadDllPath))
                {
                    return Assembly.LoadFrom(tryLoadDllPath);
                }
            }
        }
        catch { }
        return null;
    }

    #endregion Private 方法
}
