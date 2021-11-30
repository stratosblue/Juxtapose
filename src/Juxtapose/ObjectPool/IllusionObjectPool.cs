using System;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool
{
    /// <summary>
    /// 幻象对象池
    /// </summary>
    public abstract class IllusionObjectPool<T> : IDisposable where T : IIllusion
    {
        #region Public 方法

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public abstract Task<T?> GetAsync(CancellationToken cancellation = default);

        /// <summary>
        /// 归还对象
        /// </summary>
        /// <param name="item"></param>
        public abstract void Return(T? item);

        #endregion Public 方法

        #region IDisposable

        private volatile bool _isDisposed;

        /// <summary>
        ///
        /// </summary>
        ~IllusionObjectPool()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        ///
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(IllusionObjectPool<T>));
            }
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;

            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}