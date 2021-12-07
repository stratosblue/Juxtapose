using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool
{
    /// <summary>
    /// 动态大小对象池
    /// </summary>
    public sealed class NullDynamicObjectPool<T>
        : IDynamicObjectPool<T>
    {
        #region Public 属性

        /// <summary>
        /// 公共实例
        /// </summary>
        public static IDynamicObjectPool<T> Instance { get; } = new NullDynamicObjectPool<T>();

        #endregion Public 属性

        #region Private 构造函数

        private NullDynamicObjectPool()
        {
        }

        #endregion Private 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public Task<T?> RentAsync(CancellationToken cancellation = default)
        {
            return Task.FromResult<T?>(default);
        }

        /// <inheritdoc/>
        public void Return(T? instance)
        {
        }

        #endregion Public 方法
    }
}