using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose.ObjectPool
{
    /// <summary>
    /// 幻象对象池
    /// </summary>
    public abstract class IllusionObjectPool<T> 
        : IIllusionObjectPool<T>, IDisposable 
        where T : IIllusion
    {
        #region Public 方法

        /// <inheritdoc/>
        public abstract Task<T?> GetAsync(CancellationToken cancellation = default);

        /// <inheritdoc/>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        ///
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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