using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

using Juxtapose.Utils;

namespace Juxtapose
{
    /// <summary>
    /// Debugger附加器
    /// </summary>
    public static class JuxtaposeDebuggerAttacher
    {
        #region Public 方法

        /// <summary>
        /// 尝试附加到父进程的Debugger
        /// </summary>
        /// <param name="args"></param>
        [Conditional("DEBUG")]
        public static void TryAttachToParent(params string[] args)
        {
            if (!File.Exists("Juxtapose.Debugger.dll")
                || !ExternalProcessArgumentUtil.TryGetJuxtaposeOptions(args, out var options)
                || !options.EnableDebugger
                || !options.ParentProcessId.HasValue)
            {
                Console.WriteLine("Can not attach debugger.");
                return;
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Debugger is attached for current process \"{0}\".", Environment.ProcessId);
                return;
            }

            var assemblyLoadContext = new AssemblyLoadContext("JuxtaposeDebuggerAttacher", true);

            try
            {
                Console.WriteLine("Try attach to debugger dynamic.");

                using var dllStream = File.OpenRead("Juxtapose.Debugger.dll");
                var assembly = assemblyLoadContext.LoadFromStream(dllStream);

                assembly.GetType("System.DebuggerAttachHelper", false, true)
                        ?.GetMethod("AttachTo", BindingFlags.Public | BindingFlags.Static)
                        ?.Invoke(null, new object[] { options.ParentProcessId.Value });
            }
            finally
            {
                assemblyLoadContext.Unload();
            }
        }

        #endregion Public 方法
    }
}