using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace System
{
    public static partial class SystemInfo
    {
        #region Public 类

        /// <summary>
        /// 内存信息
        /// </summary>
        public static class MemoryInfo
        {
            private static IMemoryInfo? s_memoryInfo;

            private static IMemoryInfo InternalMemoryInfo => s_memoryInfo ??= GetMemoryInfo();

            #region Public 属性

            /// <summary>
            /// 可用内存大小
            /// </summary>
            public static StorageSize Available => InternalMemoryInfo.Available;

            /// <summary>
            /// 总内存大小
            /// </summary>
            public static StorageSize Total => InternalMemoryInfo.Total;

            /// <summary>
            /// 已使用内存大小
            /// </summary>
            public static StorageSize Usage => InternalMemoryInfo.Usage;

            #endregion Public 属性

            #region Public 方法

            /// <summary>
            /// 展示内存信息到标准输出流
            /// </summary>
            public static void Display()
            {
                Console.WriteLine($"Total: {Total}");
                Console.WriteLine($"Usage: {Usage}");
                Console.WriteLine($"Available: {Available}");
            }

            #endregion Public 方法

            #region Private

            private static IMemoryInfo GetMemoryInfo()
            {
                return Platform switch
                {
                    PlatformType.Windows => new WindowsMemoryInfo(),
                    PlatformType.Linux => new LinuxMemoryInfo(),
                    _ => throw new PlatformNotSupportedException(),
                };
            }

            #endregion Private

            #region MemoryInfoImpl

            private interface IMemoryInfo
            {
                StorageSize Available { get; }

                StorageSize Total { get; }

                StorageSize Usage { get; }
            }

            private sealed class LinuxMemoryInfo : IMemoryInfo
            {
                /// <summary>
                /// 无效的内存大小
                /// </summary>
                private const long InvalidMemorySize = 9223372036854771712;

                private static readonly Regex s_availableMemoryRegex = new("MemAvailable:.+?(\\d+).+?kB", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));
                private static readonly Regex s_totalMemoryRegex = new("MemTotal:.+?(\\d+).+?kB", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));

                public StorageSize Available
                {
                    get
                    {
                        if (IsContainer)
                        {
                            return Total - Usage;
                        }

                        return MatchKiloByteSizeFromFile("/proc/meminfo", s_availableMemoryRegex);
                    }
                }

                public StorageSize Total { get; } = GetTotalMemory();

                public StorageSize Usage
                {
                    get
                    {
                        if (IsContainer)
                        {
                            if (ReadByteSizeFromFile("/sys/fs/cgroup/memory/memory.usage_in_bytes") is StorageSize storageSize)
                            {
                                return storageSize;
                            }
                            throw new PlatformNotSupportedException();
                        }

                        return Total - Available;
                    }
                }

                private static StorageSize GetTotalMemory()
                {
                    if (IsContainer)
                    {
                        if (ReadByteSizeFromFile("/sys/fs/cgroup/memory/memory.limit_in_bytes") is StorageSize storageSize)
                        {
                            return storageSize;
                        }
                    }
                    return MatchKiloByteSizeFromFile("/proc/meminfo", s_totalMemoryRegex);
                }

                private static StorageSize MatchKiloByteSizeFromFile(string filePath, Regex regex)
                {
                    return MatchKiloByteSizeFromText(File.ReadAllText(filePath), regex);
                }

                private static StorageSize MatchKiloByteSizeFromText(string text, Regex regex)
                {
                    var KiloByteText = regex.Match(text).Groups[1].Value;
                    return new StorageSize(long.Parse(KiloByteText) * 1024);
                }

                /// <summary>
                /// 从文件读取大小
                /// </summary>
                /// <param name="filePath"></param>
                /// <returns></returns>
                private static StorageSize? ReadByteSizeFromFile(string filePath)
                {
                    var size = File.ReadAllText(filePath);
                    var sizeValue = long.Parse(size);
                    if (InvalidMemorySize != sizeValue)
                    {
                        return new StorageSize(sizeValue);
                    }

                    return null;
                }
            }

            private sealed class WindowsMemoryInfo : IMemoryInfo
            {
                public StorageSize Available => WindowsMemoryInfoHelper.GetMemoryInfo().AvailablePhysical;

                public StorageSize Total { get; } = WindowsMemoryInfoHelper.GetMemoryInfo().TotalPhysical;

                public StorageSize Usage
                {
                    get
                    {
                        var memoryInfo = WindowsMemoryInfoHelper.GetMemoryInfo();
                        return memoryInfo.TotalPhysical - memoryInfo.AvailablePhysical;
                    }
                }
            }

            #endregion MemoryInfoImpl
        }

        #endregion Public 类
    }

    #region Platform

    #region Windows

    /// <summary>
    /// Windows原生内存信息<para/>
    /// See https://docs.microsoft.com/en-us/windows/win32/api/sysinfoapi/ns-sysinfoapi-memorystatusex
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct WindowsNativeMemoryInfo
    {
        private readonly uint _dwLength = sizeof(uint) * 2 + sizeof(ulong) * 7;

        /// <summary>
        /// 内存使用百分比
        /// </summary>
        public readonly uint MemoryLoad = default;

        /// <summary>
        /// 总物理内存
        /// </summary>
        public readonly StorageSize TotalPhysical = default;

        /// <summary>
        /// 可用物理内存
        /// </summary>
        public readonly StorageSize AvailablePhysical = default;

        /// <summary>
        /// 总页面文件
        /// </summary>
        public readonly StorageSize TotalPageFile = default;

        /// <summary>
        /// 可用页面文件
        /// </summary>
        public readonly StorageSize AvailablePageFile = default;

        /// <summary>
        /// 总虚拟内存
        /// </summary>
        public readonly StorageSize TotalVirtual = default;

        /// <summary>
        /// 可用虚拟内存
        /// </summary>
        public readonly StorageSize AvailableVirtual = default;

        private readonly ulong _ullAvailExtendedVirtual = default;

        /// <inheritdoc cref="WindowsNativeMemoryInfo"/>
        public WindowsNativeMemoryInfo()
        {
        }
    }

    /// <summary>
    /// Windows内存信息帮助类
    /// </summary>
    public static class WindowsMemoryInfoHelper
    {
        /// <summary>
        /// 获取内存信息
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static WindowsNativeMemoryInfo GetMemoryInfo()
        {
            var buffer = new WindowsNativeMemoryInfo();

            if (!GlobalMemoryStatusEx(ref buffer))
            {
                throw new InvalidOperationException("Call GlobalMemoryStatusEx failed.");
            }
            return buffer;
        }

        /// <summary>
        /// See https://docs.microsoft.com/en-us/windows/win32/api/sysinfoapi/nf-sysinfoapi-globalmemorystatusex
        /// </summary>
        /// <param name="lpBuffer"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "GlobalMemoryStatusEx")]
        private static extern bool GlobalMemoryStatusEx(ref WindowsNativeMemoryInfo lpBuffer);
    }

    #endregion Windows

    #endregion Platform
}