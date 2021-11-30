using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool
{
    /// <inheritdoc cref="LimitedIllusionObjectPool{T}"/>
    public static class LimitedIllusionObjectPool
    {
        #region Public 方法

        /// <inheritdoc cref="DefaultLimitedIllusionObjectPool{T}.DefaultLimitedIllusionObjectPool"/>
        public static LimitedIllusionObjectPool<T> Create<T>(Func<CancellationToken, Task<T?>> objectCreateFunc,
                                                              Func<T, bool> checkShouldDestroyFunc,
                                                              int retainedObjectCount,
                                                              int maximumObjectCount,
                                                              bool blockWhenNoAvailable)
            where T : IIllusion
        {
            return new DefaultLimitedIllusionObjectPool<T>(objectCreateFunc, checkShouldDestroyFunc, retainedObjectCount, maximumObjectCount, blockWhenNoAvailable);
        }

        #endregion Public 方法
    }

    /// <summary>
    /// 有限大小幻象对象池
    /// </summary>
    public abstract class LimitedIllusionObjectPool<T>
        : IllusionObjectPool<T>
        where T : IIllusion
    {
        #region Private 字段

        private readonly SemaphoreSlim? _getObjectSemaphore;
        private volatile int _currentCount;

        #endregion Private 字段

        #region Protected 字段

        /// <summary>
        /// 没有可用对象时阻塞
        /// </summary>
        protected readonly bool BlockWhenNoAvailable;

        /// <summary>
        /// 对象队列
        /// </summary>
        protected readonly ConcurrentQueue<T> ObjectQueue = new();

        /// <summary>
        /// 释放获取对象锁
        /// </summary>
        protected readonly Action ReleaseGetObjectLock;

        /// <summary>
        /// 等待获取对象锁
        /// </summary>
        protected readonly Func<CancellationToken, Task> WaitGetObjectLockAsync;

        #endregion Protected 字段

        #region Public 属性

        /// <summary>
        /// 当前数量
        /// </summary>
        public int CurrentCount => _currentCount;

        /// <summary>
        /// 最大可创建对象数量
        /// </summary>
        public int MaximumObjectCount { get; }

        /// <summary>
        /// 可保留的对象总数
        /// </summary>
        public int RetainedObjectCount { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="LimitedIllusionObjectPool{T}"/>
        /// </summary>
        /// <param name="retainedObjectCount">保留总数</param>
        /// <param name="maximumObjectCount">最大总数(-1为不限制)</param>
        /// <param name="blockWhenNoAvailable">没有可用对象时，阻塞进行等待</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public LimitedIllusionObjectPool(int retainedObjectCount, int maximumObjectCount, bool blockWhenNoAvailable)
        {
            if (retainedObjectCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retainedObjectCount), $"{nameof(retainedObjectCount)} must bigger than 0.");
            }
            if (maximumObjectCount != -1) //有最大数量限制
            {
                if (maximumObjectCount < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(maximumObjectCount), $"{nameof(maximumObjectCount)} must bigger than 0 or equal -1.");
                }
                if (retainedObjectCount > maximumObjectCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(retainedObjectCount), $"{nameof(retainedObjectCount)} must less than {nameof(maximumObjectCount)}.");
                }
            }

            RetainedObjectCount = retainedObjectCount;
            MaximumObjectCount = maximumObjectCount;

            BlockWhenNoAvailable = blockWhenNoAvailable;

            if (blockWhenNoAvailable
                && maximumObjectCount != -1)
            {
                _getObjectSemaphore = new SemaphoreSlim(maximumObjectCount, maximumObjectCount);
                WaitGetObjectLockAsync = cancellation => _getObjectSemaphore!.WaitAsync(cancellation);
                ReleaseGetObjectLock = () => _getObjectSemaphore!.Release();
            }
            else
            {
                WaitGetObjectLockAsync = static _ => Task.CompletedTask;
                ReleaseGetObjectLock = static () => { };
            }
        }

        #endregion Public 构造函数

        #region Protected 方法

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        protected abstract Task<T?> CreateAsync(CancellationToken cancellation = default);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _getObjectSemaphore?.Dispose();

            while (ObjectQueue.TryDequeue(out var item))
            {
                item.Dispose();
            }
        }

        /// <summary>
        /// 检查是否应该销毁实例
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        protected abstract bool ShouldDestroy(T instance);

        #endregion Protected 方法

        #region Public 方法

        /// <inheritdoc/>
        public override async Task<T?> GetAsync(CancellationToken cancellation = default)
        {
            ThrowIfDisposed();

            await WaitGetObjectLockAsync(cancellation);

            try
            {
                if (ObjectQueue.TryDequeue(out var item))
                {
                    return item;
                }

                if (MaximumObjectCount != -1
                    && _currentCount >= MaximumObjectCount)
                {
                    return default;
                }

                item = await CreateAsync(cancellation);

                if (item is null)
                {
                    ReleaseGetObjectLock();
                    return item;
                }

                Interlocked.Increment(ref _currentCount);
                return item;
            }
            catch
            {
                ReleaseGetObjectLock();
                throw;
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
                    if (_currentCount <= RetainedObjectCount
                        && !ShouldDestroy(item))
                    {
                        ObjectQueue.Enqueue(item);
                    }
                    else
                    {
                        Interlocked.Decrement(ref _currentCount);
                        item.Dispose();
                    }
                }
                else
                {
                    Interlocked.Decrement(ref _currentCount);
                }
            }
            catch
            {
                Interlocked.Decrement(ref _currentCount);
                item.Dispose();
                throw;
            }
            finally
            {
                ReleaseGetObjectLock();
            }
        }

        #endregion Public 方法
    }
}