using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// 系统信息
    /// </summary>
    public static partial class SystemInfo
    {
        #region Public 属性

        /// <summary>
        /// 是否在容器内
        /// </summary>
        public static bool IsContainer { get; } = CheckIsContainer();

        /// <summary>
        /// 当前平台
        /// </summary>
        public static PlatformType Platform => PlatformTypeInstanceClass.Platform;

        #endregion Public 属性

        #region Method

        /// <summary>
        /// 展示系统信息到标准输出流
        /// </summary>
        public static void Display()
        {
            Console.WriteLine("---- SystemInfo ----");
            Console.WriteLine($"Platform: {Platform}");
            Console.WriteLine($"IsContainer: {IsContainer}");
            Console.WriteLine("---- Memory ----");
            MemoryInfo.Display();
        }

        #region private

        /// <summary>
        /// 检查是否在容器中
        /// </summary>
        /// <returns></returns>
        private static bool CheckIsContainer()
        {
            switch (Platform)
            {
                case PlatformType.Windows:

                    //SEE https://stackoverflow.com/questions/43002803/detect-if-process-executes-inside-a-windows-container
                    return WindowsServiceInfoHelper.IsServiceRunning("cexecsvc") == true;

                case PlatformType.Linux:

                    //SEE https://stackoverflow.com/questions/20010199/how-to-determine-if-a-process-runs-inside-lxc-docker
                    //MAYBE https://stackoverflow.com/questions/67155739/how-to-check-if-process-runs-within-a-docker-container-cgroup-v2-linux-host

                    var lines = File.ReadAllLines("/proc/1/cgroup");
                    return lines.Count(m => m.Count(m => m == '/') > 1) > 1;
            }
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// 获取操作系统类型
        /// </summary>
        /// <returns></returns>
        private static PlatformType GetOSPlatform()
        {
            //HACK .net standard下 OperatingSystem.IsLinux() 和 OperatingSystem.IsWindows() 不可用

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return PlatformType.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return PlatformType.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return PlatformType.MacOSX;
            }
            throw new PlatformNotSupportedException();
        }

        private static class PlatformTypeInstanceClass
        {
            #region Public 字段

            public static PlatformType Platform { get; } = GetOSPlatform();

            #endregion Public 字段
        }

        #endregion private

        #endregion Method
    }

    #region Platform

    #region Windows

    /// <summary>
    /// Windows服务信息帮助类
    /// </summary>
    public static class WindowsServiceInfoHelper
    {
        /// <summary>
        /// 检查服务是否在运行
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns>返回NULL则为检查失败，无法访问或没有服务</returns>
        public static bool? IsServiceRunning(string serviceName)
        {
            var scManagerHandle = OpenSCManagerW(IntPtr.Zero, IntPtr.Zero, 0x0001);
            if (scManagerHandle == IntPtr.Zero)
            {
                return null;
            }
            try
            {
                var scHandle = OpenServiceW(scManagerHandle, serviceName, 0x0004);

                if (scManagerHandle == IntPtr.Zero)
                {
                    return null;
                }
                try
                {
                    var lpBuffer = new SERVICE_STATUS_PROCESS();
                    if (QueryServiceStatusEx(scHandle, 0, ref lpBuffer, sizeof(uint) * 9, out _))
                    {
                        return lpBuffer.dwCurrentState == SERVICE_CURRENT_STATE.SERVICE_RUNNING;
                    }
                    return null;
                }
                finally
                {
                    CloseServiceHandle(scHandle);
                }
            }
            finally
            {
                CloseServiceHandle(scManagerHandle);
            }
        }

        #region P/Invoke

        /// <summary>
        /// See https://docs.microsoft.com/en-us/windows/win32/api/winsvc/nf-winsvc-closeservicehandle
        /// </summary>
        /// <param name="hSCObject"></param>
        /// <returns></returns>
        [DllImport("Advapi32.dll", EntryPoint = "CloseServiceHandle")]
        private static extern bool CloseServiceHandle(IntPtr hSCObject);

        /// <summary>
        /// See https://docs.microsoft.com/en-us/windows/win32/api/winsvc/nf-winsvc-openscmanagerw
        /// </summary>
        /// <param name="lpMachineName"></param>
        /// <param name="lpDatabaseName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <returns></returns>
        [DllImport("Advapi32.dll", EntryPoint = "OpenSCManagerW", CharSet = CharSet.Unicode)]
        private static extern IntPtr OpenSCManagerW(IntPtr lpMachineName, IntPtr lpDatabaseName, int dwDesiredAccess);

        /// <summary>
        /// See https://docs.microsoft.com/en-us/windows/win32/api/winsvc/nf-winsvc-openscmanagerw
        /// See https://docs.microsoft.com/en-us/windows/win32/services/service-security-and-access-rights
        /// </summary>
        /// <param name="hSCManager"></param>
        /// <param name="lpServiceName"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <returns></returns>
        [DllImport("Advapi32.dll", EntryPoint = "OpenServiceW", CharSet = CharSet.Unicode)]
        private static extern IntPtr OpenServiceW(IntPtr hSCManager, string lpServiceName, int dwDesiredAccess);

        /// <summary>
        /// See https://docs.microsoft.com/zh-cn/windows/win32/api/winsvc/nf-winsvc-queryservicestatusex
        /// </summary>
        /// <param name="hService"></param>
        /// <param name="infoLevel"></param>
        /// <param name="lpBuffer"></param>
        /// <param name="cbBufSize"></param>
        /// <param name="pcbBytesNeeded"></param>
        /// <returns></returns>
        [DllImport("Advapi32.dll", EntryPoint = "QueryServiceStatusEx", CharSet = CharSet.Unicode)]
        private static extern bool QueryServiceStatusEx(IntPtr hService, int infoLevel, ref SERVICE_STATUS_PROCESS lpBuffer, int cbBufSize, out int pcbBytesNeeded);

        #endregion P/Invoke

        #region structs

        private enum SERVICE_CURRENT_STATE : uint
        {
            SERVICE_CONTINUE_PENDING = 0x00000005,

            SERVICE_PAUSE_PENDING = 0x00000006,

            SERVICE_PAUSED = 0x00000007,

            SERVICE_RUNNING = 0x00000004,

            SERVICE_START_PENDING = 0x00000002,

            SERVICE_STOP_PENDING = 0x00000003,

            SERVICE_STOPPED = 0x00000001,
        }

        /// <summary>
        /// See https://docs.microsoft.com/en-us/windows/win32/api/winsvc/ns-winsvc-service_status_process
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_STATUS_PROCESS
        {
            public uint dwServiceType;
            public SERVICE_CURRENT_STATE dwCurrentState;
            public uint dwControlsAccepted;
            public uint dwWin32ExitCode;
            public uint dwServiceSpecificExitCode;
            public uint dwCheckPoint;
            public uint dwWaitHint;
            public uint dwProcessId;
            public uint dwServiceFlags;
        }

        #endregion structs
    }

    #endregion Windows

    #endregion Platform
}