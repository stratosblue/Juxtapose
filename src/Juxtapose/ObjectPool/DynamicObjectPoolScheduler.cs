using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool
{
    /// <inheritdoc cref="IDynamicObjectPoolScheduler{T}"/>
    public abstract class DynamicObjectPoolScheduler<T>
        : IDynamicObjectPoolScheduler<T>, IDisposable
    {
        #region Public 事件

        /// <inheritdoc/>
        public event ResourcePressureDelegate? OnResourcePressure;

        #endregion Public 事件

        #region Private 字段

        private readonly SemaphoreSlim _lockSemaphore = new(1, 1);

        private readonly CancellationTokenSource _runningTokenSource;

        private volatile bool _isDisposed;

        #endregion Private 字段

        #region Protected 字段

        /// <summary>
        /// 运行Token
        /// </summary>
        protected readonly CancellationToken RunningToken;

        #endregion Protected 字段

        #region Protected 属性

        /// <summary>
        /// 是否已释放
        /// </summary>
        protected virtual bool IsDisposed => _isDisposed;

        #endregion Protected 属性

        #region Public 构造函数

        /// <inheritdoc cref="DynamicObjectPoolScheduler{T}"/>
        public DynamicObjectPoolScheduler()
        {
            _runningTokenSource = new();
            RunningToken = _runningTokenSource.Token;
        }

        #endregion Public 构造函数

        #region Protected 方法

        /// <summary>
        /// 触发资源紧张事件
        /// </summary>
        /// <param name="level"></param>
        protected virtual void TriggerResourcePressure(ResourcePressureLevel level)
        {
            OnResourcePressure?.Invoke(level);
        }

        #endregion Protected 方法

        #region Public 方法

        /// <inheritdoc/>
        public abstract ValueTask<bool> CanCreateAsync(CancellationToken cancellation = default);

        /// <inheritdoc/>
        public virtual Task LockAsync(CancellationToken cancellation) => _lockSemaphore.WaitAsync(cancellation);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void OnCreated(T instance)
        { }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void OnDestroyed(T instance)
        { }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool OnRent(T instance) => true;

        /// <inheritdoc/>
        public abstract bool OnReturn(T instance);

        /// <inheritdoc/>
        public virtual void ReleaseLock() => _lockSemaphore.Release();

        #endregion Public 方法

        #region IDisposable

        /// <summary>
        ///
        /// </summary>
        ~DynamicObjectPoolScheduler()
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

                _lockSemaphore.Dispose();

                _runningTokenSource.Cancel();
                _runningTokenSource.Dispose();

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
                throw new ObjectDisposedException(nameof(DynamicObjectPoolScheduler<T>));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}