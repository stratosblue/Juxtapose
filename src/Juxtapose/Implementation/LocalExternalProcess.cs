using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Juxtapose;

/// <summary>
/// 本地<inheritdoc cref="IExternalProcess"/>
/// </summary>
public class LocalExternalProcess(ProcessStartInfo processStartInfo)
    : KeepRunningObject, IExternalProcess
{
    #region Public 事件

    /// <inheritdoc/>
    public event Action<IExternalProcess>? OnProcessInvalid;

    #endregion Public 事件

    #region Private 字段

    private readonly ProcessStartInfo _processStartInfo = processStartInfo ?? throw new ArgumentNullException(nameof(processStartInfo));

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
    public bool HasExited => _exitCode.HasValue || GetRequiredProcess().HasExited;

    /// <inheritdoc/>
    public int Id => GetCachedValue(() => GetRequiredProcess().Id, ref _processId);

    /// <inheritdoc/>
    public bool IsAlive => _isInitialized && _isInvalid == 0 && (!_exitCode.HasValue && Process?.HasExited == false);

    /// <inheritdoc cref="LocalExternalProcess"/>
    public Process? Process { get; private set; }

    /// <inheritdoc/>
    public DateTime StartTime => GetCachedValue(() => GetRequiredProcess().StartTime, ref _startTime);

    #endregion Public 属性

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
            if (process.Start())
            {
                GetCachedValue(() => process.Id, ref _processId);

                _ = process.WaitForExitAsync(RunningToken)
                           .ContinueWith(_ => Dispose(), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
            }
            else
            {
                throw new JuxtaposeException("Start process fail.");
            }
        }
        catch
        {
            Dispose();
            throw;
        }

        Process = process;

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override string? ToString()
    {
        if (Process is { } process)
        {
            if (_exitCode.HasValue
                || process.HasExited)
            {
                return $"Process {Id} [ExitCode: {_exitCode ?? process.ExitCode}]";
            }
            else
            {
                return $"Process {Id} [Alive]";
            }
        }
        return base.ToString();
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
                    if (disposing)
                    {
                        //只在手动释放时等待退出，5s是否过长或过短？
                        process.WaitForExit(5_000);
                    }
                }
                catch { }
                finally
                {
                    if (disposing)
                    {
                        //释放进程后可能会无法访问相关信息，尝试提前进行缓存，并忽略可能的异常
                        TryAccessCache(() => ExitCode);
                        TryAccessCache(() => StartTime);
                    }
                    process.Dispose();
                }
            }

            OnProcessInvalid = null;

            return true;
        }
        return false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void TryAccessCache<T>(Func<T> action)
        {
            try
            {
                action();
            }
            catch
            {
                //忽略所有异常
            }
        }
    }

    #endregion Dispose
}
