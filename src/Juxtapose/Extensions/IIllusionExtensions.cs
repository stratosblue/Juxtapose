using System.Diagnostics.CodeAnalysis;

namespace Juxtapose;

/// <summary>
///
/// </summary>
public static class IIllusionExtensions
{
    #region Public 方法

    /// <summary>
    /// 尝试获取关联的<see cref="IExternalProcess"/>
    /// </summary>
    /// <param name="illusion"></param>
    /// <param name="externalProcess"></param>
    /// <returns></returns>
    public static bool TryGetExternalProcess(this IIllusion illusion, [NotNullWhen(true)] out IExternalProcess? externalProcess)
    {
        //如果目标已定义为 IExternalProcessHolder 则不再做其它尝试
        if (illusion is IExternalProcessHolder externalProcessHolder)
        {
            externalProcess = externalProcessHolder.ExternalProcess;
            return externalProcess is not null;
        }
        return illusion.Executor.TryGetExternalProcess(out externalProcess);
    }

    #endregion Public 方法
}
