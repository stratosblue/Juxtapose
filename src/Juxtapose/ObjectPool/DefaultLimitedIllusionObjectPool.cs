using System;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool
{
    /// <summary>
    /// 默认的<inheritdoc cref="LimitedIllusionObjectPool{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultLimitedIllusionObjectPool<T>
          : LimitedIllusionObjectPool<T>
          where T : IIllusion
    {
        #region Private 字段

        private readonly Func<T, bool> _checkShouldDestroyFunc;
        private readonly Func<CancellationToken, Task<T?>> _objectCreateFunc;

        #endregion Private 字段

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="DefaultLimitedIllusionObjectPool{T}"/>
        /// </summary>
        /// <param name="objectCreateFunc">对象创建委托</param>
        /// <param name="checkShouldDestroyFunc">检查对象是否应该销毁委托</param>
        /// <param name="retainedObjectCount">保留总数</param>
        /// <param name="maximumObjectCount">最大总数(-1为不限制)</param>
        /// <param name="blockWhenNoAvailable">没有可用对象时，阻塞进行等待</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultLimitedIllusionObjectPool(Func<CancellationToken, Task<T?>> objectCreateFunc, Func<T, bool> checkShouldDestroyFunc, int retainedObjectCount, int maximumObjectCount, bool blockWhenNoAvailable)
            : base(retainedObjectCount, maximumObjectCount, blockWhenNoAvailable)
        {
            _objectCreateFunc = objectCreateFunc ?? throw new ArgumentNullException(nameof(objectCreateFunc));
            _checkShouldDestroyFunc = checkShouldDestroyFunc ?? throw new ArgumentNullException(nameof(checkShouldDestroyFunc));
        }

        #endregion Public 构造函数

        #region Protected 方法

        /// <inheritdoc/>
        protected override Task<T?> CreateAsync(CancellationToken cancellation = default) => _objectCreateFunc(cancellation);

        /// <inheritdoc/>
        protected override bool ShouldDestroy(T item) => _checkShouldDestroyFunc(item);

        #endregion Protected 方法
    }
}