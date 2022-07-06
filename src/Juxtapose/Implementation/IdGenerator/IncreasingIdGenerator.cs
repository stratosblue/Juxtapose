using System.Threading;

namespace Juxtapose;

/// <summary>
/// 递增的<see cref="IUniqueIdGenerator"/>
/// </summary>
public class IncreasingIdGenerator : IUniqueIdGenerator
{
    #region Private 字段

    private readonly object _syncRoot = new();
    private int _id = 0;

    #endregion Private 字段

    #region Public 方法

    /// <inheritdoc/>
    public int Next()
    {
        var id = Interlocked.Increment(ref _id);
        if (id > Constants.IdThreshold)
        {
            lock (_syncRoot)
            {
                if (Volatile.Read(ref _id) > Constants.IdThreshold)
                {
                    Volatile.Write(ref _id, 0);
                }
            }
        }
        return id;
    }

    #endregion Public 方法
}