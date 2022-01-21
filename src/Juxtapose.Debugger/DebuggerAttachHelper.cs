using System.Runtime.Versioning;

namespace System
{
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
            VsDebuggerAttacher.AttachToTargetProcessDebugger(Environment.ProcessId, targetProcessId);
        }

        #endregion Public 方法
    }
}