using System.Diagnostics;
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

                    var output = GetWmicOutput("service where \"Name = 'cexecsvc'\" get Started");
                    return output.Contains("Started", StringComparison.Ordinal);

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

        /// <summary>
        /// 获取 WMIC 查询输出
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static string GetWmicOutput(string query)
        {
            var info = new ProcessStartInfo("wmic")
            {
                UseShellExecute = false,
                Arguments = query,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var output = string.Empty;
            using (var process = Process.Start(info))
            {
                output = process!.StandardOutput.ReadToEnd();
            }
            return output.Trim();
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
}