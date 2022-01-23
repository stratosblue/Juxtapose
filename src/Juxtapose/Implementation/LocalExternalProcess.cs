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
    public class LocalExternalProcess : KeepRunningObject, IExternalProcess
    {
        #region Public 事件

        /// <inheritdoc/>
        public event Action<IExternalProcess>? OnProcessInvalid;

        #endregion Public 事件

        #region Private 字段

        private readonly ProcessStartInfo _processStartInfo;
        private int? _exitCode;
        private volatile bool _isInitialized;
        private int _isInvalid;
        private int? _processId;
        private DateTime? _startTime;

        #endregion Private 字段

        #region Public 属性

        /// <inheritdoc/>
        public int ExitCode => GetCachedValue(() => GetRequiredProcess().ExitCode, ref _exitCode);

        /// <inheritdoc/>
        public bool HasExited => GetRequiredProcess().HasExited;

        /// <inheritdoc/>
        public int Id => GetCachedValue(() => GetRequiredProcess().Id, ref _processId);

        /// <inheritdoc/>
        public bool IsAlive => _isInitialized && _isInvalid == 0 && Process?.HasExited == false;

        /// <inheritdoc cref="LocalExternalProcess"/>
        public Process? Process { get; private set; }

        /// <inheritdoc/>
        public DateTime StartTime => GetCachedValue(() => GetRequiredProcess().StartTime, ref _startTime);

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="LocalExternalProcess"/>
        public LocalExternalProcess(ProcessStartInfo processStartInfo)
        {
            _processStartInfo = processStartInfo ?? throw new ArgumentNullException(nameof(processStartInfo));
        }

        #endregion Public 构造函数

        #region Private 方法

        private static T GetCachedValue<T>(Func<T> getFunc, ref T? cacheValue) where T : struct
        {
            return cacheValue 
                   ?? (cacheValue = new T?(getFunc())).Value;
        }

        private Process GetRequiredProcess()
        {
            if (Process is Process process)
            {
                return process;
            }
            throw new ObjectDisposedException(nameof(LocalExternalProcess));
        }

        private void SetInvalid()
        {
            if (Interlocked.Increment(ref _isInvalid) == 1
                && OnProcessInvalid is Action<IExternalProcess> onProcessInvalid)
            {
                Task.Run(() => onProcessInvalid.Invoke(this));
            }
        }

        #endregion Private 方法

        #region Public 方法

        /// <inheritdoc/>
        public long? GetMemoryUsage()
        {
            ThrowIfDisposed();
            if (Process is not null)
            {
                Process.Refresh();
                return Process.PrivateMemorySize64;
            }
            return null;
        }

        /// <inheritdoc/>
        public StreamReader? GetStandardError() => _processStartInfo.RedirectStandardError ? Process?.StandardError : null;

        /// <inheritdoc/>
        public StreamReader? GetStandardOutput() => _processStartInfo.RedirectStandardOutput ? Process?.StandardOutput : null;

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

                _ = process.WaitForExitAsync(RunningToken)
                           .ContinueWith(_ => Dispose(), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
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
        /// <param name="disposing"></param>
        protected override bool Dispose(bool disposing)
        {
            if (base.Dispose(disposing))
            {
                SetInvalid();

                if (Process is Process process)
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch { }
                    finally
                    {
                        process.Dispose();
                    }
                }

                OnProcessInvalid = null;

                return true;
            }
            return false;
        }

        #endregion Dispose
    }
}