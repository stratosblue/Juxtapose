using System.IO;
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
            #region Private 字段

            /// <summary>
            /// 无效的内存大小
            /// </summary>
            private const long InvalidMemorySize = 9223372036854771712;

            #endregion Private 字段

            #region Public 属性

            /// <summary>
            /// 可用内存大小
            /// </summary>
            public static StorageSize Available
            {
                get
                {
                    switch (Platform)
                    {
                        case PlatformType.Windows:
                            return MatchStorageSizeFromWmicQuery("OS get FreePhysicalMemory /Value", "FreePhysicalMemory=(\\d+)", value => value * 1024);

                        case PlatformType.Linux:
                            if (IsContainer)
                            {
                                return Total - Usage;
                            }

                            return MatchByteSizeFromFile("/proc/meminfo",
                                                         $"MemAvailable:.+?(\\d+).+?kB",
                                                         value => value * 1024);
                    }
                    throw new PlatformNotSupportedException();
                }
            }

            /// <summary>
            /// 总内存大小
            /// </summary>
            public static StorageSize Total { get; } = GetTotalMemory();

            /// <summary>
            /// 已使用内存大小
            /// </summary>
            public static StorageSize Usage
            {
                get
                {
                    switch (Platform)
                    {
                        case PlatformType.Windows:
                            return Total - Available;

                        case PlatformType.Linux:
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
                    throw new PlatformNotSupportedException();
                }
            }

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

            /// <summary>
            /// 获取系统内存总量
            /// </summary>
            /// <returns></returns>
            private static StorageSize GetTotalMemory()
            {
                switch (Platform)
                {
                    case PlatformType.Windows:
                        return MatchStorageSizeFromWmicQuery("OS get TotalVisibleMemorySize /Value", "TotalVisibleMemorySize=(\\d+)", value => value * 1024);

                    case PlatformType.Linux:
                        if (IsContainer)
                        {
                            if (ReadByteSizeFromFile("/sys/fs/cgroup/memory/memory.limit_in_bytes") is StorageSize storageSize)
                            {
                                return storageSize;
                            }
                        }
                        return MatchByteSizeFromFile("/proc/meminfo",
                                                     $"MemTotal:.+?(\\d+).+?kB",
                                                     value => value * 1024);
                }
                throw new PlatformNotSupportedException();
            }

            /// <summary>
            /// 从 文件 中匹配大小
            /// </summary>
            /// <param name="filePath"></param>
            /// <param name="regex"></param>
            /// <param name="calculateFunc"></param>
            /// <returns></returns>
            private static StorageSize MatchByteSizeFromFile(string filePath, string regex, Func<long, long> calculateFunc)
            {
                var meminfoText = File.ReadAllText(filePath);
                return MatchByteSizeFromText(meminfoText, regex, calculateFunc);
            }

            /// <summary>
            /// 从 文本 中匹配大小
            /// </summary>
            /// <param name="text"></param>
            /// <param name="regex"></param>
            /// <param name="calculateFunc"></param>
            /// <returns></returns>
            private static StorageSize MatchByteSizeFromText(string text, string regex, Func<long, long> calculateFunc)
            {
                var memTotalText = Regex.Match(text, regex, RegexOptions.IgnoreCase).Groups[1].Value;
                var value = calculateFunc(long.Parse(memTotalText));

                return new StorageSize(value);
            }

            /// <summary>
            /// 从 WMIC 查询结果中匹配大小
            /// </summary>
            /// <param name="query"></param>
            /// <param name="regex"></param>
            /// <param name="calculateFunc"></param>
            /// <returns></returns>
            private static StorageSize MatchStorageSizeFromWmicQuery(string query, string regex, Func<long, long> calculateFunc)
            {
                var output = GetWmicOutput(query);
                return MatchByteSizeFromText(output, regex, calculateFunc);
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

            #endregion Private
        }

        #endregion Public 类
    }
}