using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool
{
    /// <summary>
    /// 动态大小对象池
    /// </summary>
    public static class DynamicObjectPool
    {
        #region Public 方法

        /// <summary>
        /// 使用参数传递的委托创建一个池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scheduler">规划器</param>
        /// <param name="createDelegate">创建对象委托</param>
        /// <param name="destroyDelegate">销毁对象委托</param>
        /// <param name="resourcePressureDelegate">资源压力触发时的委托</param>
        /// <returns></returns>
        public static DynamicObjectPool<T> Create<T>(IDynamicObjectPoolScheduler<T> scheduler,
                                                     Func<CancellationToken, Task<T>> createDelegate,
                                                     Action<T> destroyDelegate,
                                                     ResourcePressureDelegate resourcePressureDelegate)
            => new DelegateBaseDynamicObjectPool<T>(scheduler, createDelegate, destroyDelegate, resourcePressureDelegate);

        #endregion Public 方法
    }

    /// <summary>
    /// 动态大小对象池
    /// </summary>
    public abstract class DynamicObjectPool<T>
        : IDynamicObjectPool<T>
    {
        #region Private 字段

        private volatile bool _isDisposed;

        #endregion Private 字段

        #region Protected 字段

        /// <summary>
        /// 对象队列
        /// </summary>
        protected readonly ConcurrentQueue<T> ObjectQueue = new();

        /// <summary>
        /// 规划器
        /// </summary>
        protected readonly IDynamicObjectPoolScheduler<T> Scheduler;

        /// <summary>
        /// 当前对象总数
        /// </summary>
        protected volatile int InternalTotalCount;

        #endregion Protected 字段

        #region Public 属性

        /// <summary>
        /// 空闲对象数量
        /// </summary>
        public int IdleCount
        {
            get
            {
                ThrowIfDisposed();
                return ObjectQueue.Count;
            }
        }

        /// <summary>
        /// 是否已释放
        /// </summary>
        public virtual bool IsDisposed => _isDisposed;

        /// <summary>
        /// 对象总数量
        /// </summary>
        public int TotalCount
        {
            get
            {
                ThrowIfDisposed();
                return InternalTotalCount;
            }
        }

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="DynamicObjectPool{T}"/>
        /// </summary>
        public DynamicObjectPool(IDynamicObjectPoolScheduler<T> scheduler)
        {
            Scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            Scheduler.OnResourcePressure += OnSchedulerResourcePressure;
        }

        #endregion Public 构造函数

        #region Protected 方法

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        protected abstract Task<T> CreateAsync(CancellationToken cancellation = default);

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="instance"></param>
        protected abstract void Destroy(T instance);

        /// <summary>
        /// 规划器触发资源压力事件时
        /// </summary>
        /// <param name="level"></param>
        protected virtual void OnSchedulerResourcePressure(ResourcePressureLevel level)
        {
        }

        #endregion Protected 方法

        #region Public 方法

        /// <inheritdoc/>
        public virtual async Task<T?> RentAsync(CancellationToken cancellation = default)
        {
            ThrowIfDisposed();

            T? item;

            //循环获取一个可用的对象
            while (!cancellation.IsCancellationRequested
                   && ObjectQueue.TryDequeue(out item))
            {
                if (!Scheduler.OnRent(item))
                {
                    Destroy(item);

                    Interlocked.Decrement(ref InternalTotalCount);

                    Scheduler.OnDestroyed(item);
                    continue;
                }
                return item;
            }

            cancellation.ThrowIfCancellationRequested();

            await Scheduler.LockAsync(cancellation);
            try
            {
                if (!await Scheduler.CanCreateAsync(cancellation))
                {
                    return default;
                }

                item = await CreateAsync(cancellation);

                if (item is null)
                {
                    throw new InvalidOperationException("Create instance fail.");
                }

                Interlocked.Increment(ref InternalTotalCount);

                Scheduler.OnCreated(item);

                return item;
            }
            finally
            {
                Scheduler.ReleaseLock();
            }
        }

        /// <inheritdoc/>
        public virtual void Return(T? item)
        {
            if (item is null)
            {
                return;
            }

            ThrowIfDisposed();

            if (Scheduler.OnReturn(item))
            {
                ObjectQueue.Enqueue(item);
            }
            else
            {
                Destroy(item);

                Interlocked.Decrement(ref InternalTotalCount);

                Scheduler.OnDestroyed(item);
            }
        }

        #region IDisposable

        /// <summary>
        ///
        /// </summary>
        ~DynamicObjectPool()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        /// <returns></returns>
        protected virtual bool Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                Scheduler.OnResourcePressure -= OnSchedulerResourcePressure;

                while (ObjectQueue.TryDequeue(out var item))
                {
                    Destroy(item);
                    Scheduler.OnDestroyed(item);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(DynamicObjectPool<T>));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #endregion Public 方法
    }
}