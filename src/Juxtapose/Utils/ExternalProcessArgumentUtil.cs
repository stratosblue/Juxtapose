using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Juxtapose.Utils;

/// <summary>
/// 外部进程启动参数工具
/// </summary>
public static class ExternalProcessArgumentUtil
{
    #region Public 字段

    /// <summary>
    /// 参数前缀
    /// </summary>
    public const string ArgumentPrefix = "juxtapose_worker:";

    #endregion Public 字段

    #region Public 方法

    /// <summary>
    /// 构建启动参数
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string BuildJuxtaposeArgument(IJuxtaposeOptions options)
    {
        options.ParentProcessId = Environment.ProcessId;

        var optionsString = options.Serialize();

        return $"{ArgumentPrefix}{Convert.ToBase64String(Encoding.UTF8.GetBytes(optionsString))}";
    }

    /// <summary>
    /// 设置为外部进程启动信息
    /// </summary>
    /// <param name="processStartInfo"></param>
    /// <param name="options"></param>
    public static void SetAsJuxtaposeProcessStartInfo(ProcessStartInfo processStartInfo, IJuxtaposeOptions options)
    {
        var argument = processStartInfo.ArgumentList.FirstOrDefault(m => m.StartsWith(ArgumentPrefix, StringComparison.OrdinalIgnoreCase));
        if (argument != null)
        {
            processStartInfo.ArgumentList.Remove(argument);
        }
        argument = BuildJuxtaposeArgument(options);

        processStartInfo.ArgumentList.Add(argument);
    }

    /// <summary>
    /// 尝试获取参数
    /// </summary>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static bool TryGetJuxtaposeOptions(string[] args, [NotNullWhen(true)] out IJuxtaposeOptions? options)
    {
        var argument = args.FirstOrDefault(m => m.StartsWith(ArgumentPrefix, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(argument))
        {
            options = null;
            return false;
        }
        options = JuxtaposeOptions.Deserialize(Encoding.UTF8.GetString(Convert.FromBase64String(argument.AsSpan().Slice(ArgumentPrefix.Length).ToString())));
        return true;
    }

    #endregion Public 方法
}