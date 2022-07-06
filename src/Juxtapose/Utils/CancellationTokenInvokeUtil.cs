using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Juxtapose.Utils;

/// <summary>
/// <see cref="CancellationToken"/> 调用工具
/// </summary>
public static class CancellationTokenInvokeUtil
{
    #region Public 方法

    /// <summary>
    /// 尝试重构为本地的代理<see cref="CancellationTokenSource"/>
    /// </summary>
    /// <param name="cancellationTokenSourceId"></param>
    /// <param name="executor"></param>
    /// <param name="cancellationTokenSource"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static bool TryRebuildCancellationTokenSource(int? cancellationTokenSourceId,
                                                         JuxtaposeExecutor executor,
                                                         [NotNullWhen(true)] out CancellationTokenSource? cancellationTokenSource,
                                                         out CancellationToken cancellationToken)
    {
        if (cancellationTokenSourceId.HasValue)
        {
            cancellationTokenSource = new CancellationTokenSource();
            executor.AddObjectInstance(cancellationTokenSourceId.Value, cancellationTokenSource);
            cancellationToken = cancellationTokenSource.Token;
            return true;
        }

        cancellationTokenSource = null;
        cancellationToken = CancellationToken.None;
        return false;
    }

    #endregion Public 方法
}