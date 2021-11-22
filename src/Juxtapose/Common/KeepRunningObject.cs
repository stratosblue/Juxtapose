using System;
using System.Diagnostics;
using System.Threading;

namespace Juxtapose
{
    /// <summary>
    /// 保持运行的对象
    /// </summary>
    public abstract class KeepRunningObject : IDisposable
    {
        #region Private 字段

        private readonly CancellationTokenSource _runningCancellationTokenSource;

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 是否已释放
        /// </summary>
        public bool IsDisposed { [DebuggerStepThrough]get; [DebuggerStepThrough]private set; }

        /// <summary>
        /// 运行的Token
        /// </summary>
        public CancellationToken RunningToken { [DebuggerStepThrough]get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="KeepRunningObject"/>
        [DebuggerStepThrough]
        public KeepRunningObject()
        {
            _runningCancellationTokenSource = new();
            RunningToken = _runningCancellationTokenSource.Token;
        }

        #endregion Public 构造函数

        #region Protected 方法

        /// <summary>
        /// 检查是否已释放
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.ToString());
            }
        }

        #endregion Protected 方法

        #region Dispose

        /// <summary>
        ///
        /// </summary>
        ~KeepRunningObject()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// <inheritdoc cref="Dispose()"/>
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual bool Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                _runningCancellationTokenSource.Cancel();
                _runningCancellationTokenSource.Dispose();
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Dispose(disposing: true))
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion Dispose
    }
}