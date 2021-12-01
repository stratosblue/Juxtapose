using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool
{
    /// <summary>
    /// 动态大小幻象对象池
    /// </summary>
    public abstract class DynamicIllusionObjectPool<T>
        : IllusionObjectPool<T>
        where T : IIllusion
    {
        #region Private 字段

        private readonly SemaphoreSlim _getObjectLockSemaphore;

        #endregion Private 字段

        #region Protected 字段

        /// <summary>
        /// 对象队列
        /// </summary>
        protected readonly ConcurrentQueue<T> ObjectQueue = new();

        /// <summary>
        /// 当前对象总数
        /// </summary>
        protected volatile int InternalTotalCount;

        #endregion Protected 字段

        #region Public 属性

        /// <inheritdoc/>
        public override int IdleCount => ObjectQueue.Count;

        /// <inheritdoc/>
        public override int TotalCount => InternalTotalCount;

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="DynamicIllusionObjectPool{T}"/>
        /// </summary>
        public DynamicIllusionObjectPool()
        {
            _getObjectLockSemaphore = new(1, 1);
        }

        #endregion Public 构造函数

        #region Protected 方法

        #region abstract

        /// <summary>
        /// 检查是否可以创建新的对象
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        protected abstract ValueTask<bool> CanCreateAsync(CancellationToken cancellation = default);

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        protected abstract Task<T> CreateAsync(CancellationToken cancellation = default);

        /// <summary>
        /// 检查是否应该销毁实例
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected abstract bool ShouldDestroy(T instance);

        #endregion abstract

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _getObjectLockSemaphore?.Dispose();

            while (ObjectQueue.TryDequeue(out var item))
            {
                item.Dispose();
            }
        }

        #endregion Protected 方法

        #region Public 方法

        /// <inheritdoc/>
        public override async Task<T?> GetAsync(CancellationToken cancellation = default)
        {
            ThrowIfDisposed();

            await _getObjectLockSemaphore.WaitAsync(cancellation);
            try
            {
                while (!cancellation.IsCancellationRequested)
                {
                    if (ObjectQueue.TryDequeue(out var item))
                    {
                        if (!item.IsAvailable)
                        {
                            Interlocked.Decrement(ref InternalTotalCount);
                            continue;
                        }
                        return item;
                    }

                    if (!await CanCreateAsync(cancellation))
                    {
                        return default;
                    }

                    item = await CreateAsync(cancellation);

                    if (item is null)
                    {
                        throw new InvalidOperationException("Create instance fail.");
                    }

                    Interlocked.Increment(ref InternalTotalCount);
                    return item;
                }
                throw new OperationCanceledException(cancellation);
            }
            finally
            {
                _getObjectLockSemaphore.Release();
            }
        }

        /// <inheritdoc/>
        public override void Return(T? item)
        {
            if (item is null)
            {
                return;
            }

            ThrowIfDisposed();

            try
            {
                if (item.IsAvailable)
                {
                    if (!ShouldDestroy(item))
                    {
                        ObjectQueue.Enqueue(item);
                    }
                    else
                    {
                        Interlocked.Decrement(ref InternalTotalCount);
                        item.Dispose();
                    }
                }
                else
                {
                    Interlocked.Decrement(ref InternalTotalCount);
                }
            }
            catch
            {
                Interlocked.Decrement(ref InternalTotalCount);
                item.Dispose();
                throw;
            }
        }

        #endregion Public 方法
    }
}