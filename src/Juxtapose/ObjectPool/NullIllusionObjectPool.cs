using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool
{
    /// <summary>
    /// 空的幻象对象池
    /// </summary>
    public sealed class NullIllusionObjectPool<T>
        : IIllusionObjectPool<T>
        where T : IIllusion
    {
        #region 共享实例

        /// <summary>
        /// 共享实例
        /// </summary>
        public static NullIllusionObjectPool<T> Instance { get; } = new();

        #endregion 共享实例

        #region Public 属性

        /// <inheritdoc/>
        public int IdleCount { get; } = 0;

        /// <inheritdoc/>
        public int TotalCount { get; } = 0;

        #endregion Public 属性

        #region Public 方法

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public Task<T?> GetAsync(CancellationToken cancellation = default)
        {
            return Task.FromResult<T?>(default);
        }

        /// <inheritdoc/>
        public void Return(T? item)
        {
        }

        #endregion Public 方法
    }
}