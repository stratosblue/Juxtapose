using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

using Juxtapose.Utils;

namespace Juxtapose;

/// <summary>
/// Debugger附加器
/// </summary>
public static class JuxtaposeDebuggerAttacher
{
    #region Public 字段

    /// <summary>
    /// VsDebugger库名称
    /// </summary>
    public const string VsDebuggerLibraryFileName = "Juxtapose.VsDebugger.dll";

    #endregion Public 字段

    #region Public 方法

    /// <summary>
    /// 尝试附加到父进程的Debugger
    /// </summary>
    /// <param name="args"></param>
    [Conditional("DEBUG")]
    public static void TryAttachToParent(params string[] args)
    {
        if (Debugger.IsAttached)
        {
            return;
        }

        if (!File.Exists(VsDebuggerLibraryFileName)
            || !ExternalProcessArgumentUtil.TryGetJuxtaposeOptions(args, out var options))
        {
            return;
        }
        TryAttachToParent(options);
    }

    #endregion Public 方法

    #region Internal 方法

    internal static void TryAttachToParent(IJuxtaposeOptions? options)
    {
        if (Debugger.IsAttached
            || !OperatingSystem.IsWindows()
            || options is null
            || !options.EnableDebugger
            || !options.ParentProcessId.HasValue)
        {
            return;
        }

        var targetProcessId = options.ParentProcessId.Value;

        var assemblyLoadContext = new AssemblyLoadContext("JuxtaposeDebuggerAttacher", true);

        try
        {
            ConsoleWriteLineWithColor($"JuxtaposeDebuggerAttacher: Try attach current process {Environment.ProcessId} to process {targetProcessId}'s debugger dynamically.", ConsoleColor.Green);

            using var dllStream = File.OpenRead(VsDebuggerLibraryFileName);
            var assembly = assemblyLoadContext.LoadFromStream(dllStream);

            var attachMethod = assembly.GetType("System.DebuggerAttachHelper", false, true)
                                       ?.GetMethod("AttachTo", BindingFlags.Public | BindingFlags.Static);
            if (attachMethod is null)
            {
                ConsoleWriteLineWithColor($"JuxtaposeDebuggerAttacher: Can not get Attach method in {VsDebuggerLibraryFileName}.", ConsoleColor.Red);
                return;
            }
            else
            {
                attachMethod.Invoke(null, new object[] { targetProcessId });
                ConsoleWriteLineWithColor($"JuxtaposeDebuggerAttacher: dynamic attach process {Environment.ProcessId} to process {targetProcessId}'s debugger done.", ConsoleColor.Green);
            }
        }
        finally
        {
            assemblyLoadContext.Unload();
        }
    }

    #endregion Internal 方法

    #region util

    private static void ConsoleWriteLineWithColor(string message, ConsoleColor color)
    {
        var colorBak = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = colorBak;
    }

    #endregion util
}