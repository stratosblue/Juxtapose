using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose
{
    /// <summary>
    /// 本地<inheritdoc cref="IExternalProcess"/>
    /// </summary>
    public class LocalExternalProcess : IExternalProcess
    {
        #region Public 事件

        /// <inheritdoc/>
        public event Action<IExternalProcess>? OnProcessInvalid;

        #endregion Public 事件

        #region Private 字段

        private readonly ProcessStartInfo _processStartInfo;
        private bool _isDisposed;
        private volatile bool _isInitialized;
        private int _isInvalid;

        #endregion Private 字段

        #region Public 属性

        /// <inheritdoc/>
        public int ExitCode
        {
            get
            {
                ThrowIfDisposed();
                return Process?.ExitCode ?? throw new NotInitializedException(nameof(LocalExternalProcess));
            }
        }

        /// <inheritdoc/>
        public bool HasExited
        {
            get
            {
                ThrowIfDisposed();
                return Process?.HasExited ?? throw new NotInitializedException(nameof(LocalExternalProcess));
            }
        }

        /// <inheritdoc/>
        public int Id
        {
            get
            {
                ThrowIfDisposed();
                return Process?.Id ?? throw new NotInitializedException(nameof(LocalExternalProcess));
            }
        }

        /// <inheritdoc/>
        public bool IsAlive => _isInitialized && _isInvalid == 0;

        /// <inheritdoc cref="LocalExternalProcess"/>
        public Process? Process { get; private set; }

        /// <inheritdoc/>
        public DateTime StartTime
        {
            get
            {
                ThrowIfDisposed();
                return Process?.StartTime ?? throw new NotInitializedException(nameof(LocalExternalProcess));
            }
        }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="LocalExternalProcess"/>
        public LocalExternalProcess(ProcessStartInfo processStartInfo)
        {
            _processStartInfo = processStartInfo ?? throw new ArgumentNullException(nameof(processStartInfo));
        }

        #endregion Public 构造函数

        #region Private 方法

        private void SetInvalid()
        {
            if (Interlocked.Increment(ref _isInvalid) == 1
                && OnProcessInvalid is Action<IExternalProcess> onProcessInvalid)
            {
                Task.Run(() => onProcessInvalid.Invoke(this));
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(LocalExternalProcess));
            }
        }

        #endregion Private 方法

        #region Public 方法

        /// <inheritdoc/>
        public long? GetMemoryUsage()
        {
            ThrowIfDisposed();
            return Process?.PrivateMemorySize64;
        }

        /// <inheritdoc/>
        public StreamReader? GetStandardError() => Process?.StandardError;

        /// <inheritdoc/>
        public StreamReader? GetStandardOutput() => Process?.StandardOutput;

        /// <inheritdoc/>
        public Task InitializationAsync(CancellationToken initializationToken)
        {
            ThrowIfDisposed();

            if (_isInitialized)
            {
                throw new InvalidOperationException($"{nameof(LocalExternalProcess)} is Initialized.");
            }
            _isInitialized = true;

            var process = new Process()
            {
                StartInfo = _processStartInfo,
            };

            try
            {
                process.Start();

                _ = process.WaitForExitAsync(CancellationToken.None).ContinueWith(_ =>
                {
                    Dispose();
                    process.Dispose();
                }, CancellationToken.None);
            }
            catch
            {
                Dispose();
                throw;
            }

            Process = process;

            return Task.CompletedTask;
        }

        #endregion Public 方法

        #region Dispose

        /// <summary>
        ///
        /// </summary>
        ~LocalExternalProcess()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                SetInvalid();

                if (Process is Process process)
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch { }
                }
                _isDisposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion Dispose
    }
}