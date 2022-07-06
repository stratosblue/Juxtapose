using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose;

/// <inheritdoc cref="IJuxtaposeBootstrapper"/>
public class JuxtaposeBootstrapper : IJuxtaposeBootstrapper
{
    #region Private 字段

    private readonly IInitializationContext _context;

    private readonly IExternalProcessActivator _externalProcessActivator;

    private readonly IMessageExchangerFactory _messageExchangerFactory;

    private bool _isDisposed;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="JuxtaposeBootstrapper"/>
    public JuxtaposeBootstrapper(IInitializationContext context, IExternalProcessActivator? externalProcessActivator = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _externalProcessActivator = externalProcessActivator ?? new LocalExternalProcessActivator();
        _messageExchangerFactory = _context.GetOrCreateMessageExchangerFactory() ?? throw new JuxtaposeException("Can not get MessageExchangerFactory from context.");
    }

    #endregion Public 构造函数

    #region Protected 方法

    /// <summary>
    /// 创建 <see cref="JuxtaposeExecutor"/>
    /// </summary>
    /// <param name="messageExchanger"></param>
    /// <param name="initializationToken"></param>
    /// <returns></returns>
    protected virtual async Task<JuxtaposeExecutor> CreateExecutorAsync(IMessageExchanger messageExchanger, CancellationToken initializationToken)
    {
        var executor = _context.CreateExecutor(messageExchanger);

        await executor.InitializationAsync(initializationToken);

        return executor;
    }

    /// <summary>
    /// 创建<see cref="IExternalWorker"/>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    protected virtual async Task<IExternalWorker> CreateExternalWorkerAsync(IJuxtaposeOptions options, CancellationToken cancellation)
    {
        IExternalProcess? externalProcess = null;
        IMessageExchanger? messageExchanger = null;
        ExternalWorker? externalWorker = null;
        try
        {
            externalProcess = _externalProcessActivator.CreateProcess(options);

#if DEBUG
            externalProcess.OnProcessInvalid += process =>
            {
                Console.WriteLine($"Process: {process.Id} Exited. HasExited: {process.HasExited}");
                if (process.HasExited)
                {
                    Console.WriteLine($"ExitCode: {process.ExitCode}");
                }
                Console.WriteLine(process.GetStandardOutput()?.ReadToEnd());
                Console.WriteLine(process.GetStandardError()?.ReadToEnd());
            };
#endif
            messageExchanger = await _messageExchangerFactory.CreateHostAsync(options, cancellation);

            externalWorker = new ExternalWorker(externalProcess, messageExchanger);
        }
        catch
        {
            externalProcess?.Dispose();
            messageExchanger?.Dispose();
            externalWorker?.Dispose();
            throw;
        }
        return externalWorker;
    }

    #endregion Protected 方法

    #region Public 方法

    #region IDisposable

    /// <summary>
    ///
    /// </summary>
    ~JuxtaposeBootstrapper()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc cref="Dispose()"/>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _messageExchangerFactory.Dispose();
        }
    }

    /// <summary>
    ///
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(JuxtaposeBootstrapper));
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    /// <inheritdoc/>
    public virtual async Task<JuxtaposeExecutor> PrepareExecutorAsync(IJuxtaposeOptions options, CancellationToken initializationToken)
    {
        ThrowIfDisposed();
        var messageExchanger = await _messageExchangerFactory.CreateAsync(options, initializationToken);

        return await CreateExecutorAsync(messageExchanger, initializationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<JuxtaposeExecutor> PrepareHostingExecutorAsync(CancellationToken initializationToken)
    {
        ThrowIfDisposed();
        var options = new JuxtaposeOptions(new Dictionary<string, string?>(_context.Options));

        var externalWorker = await CreateExternalWorkerAsync(options, initializationToken);

        return await CreateExecutorAsync(externalWorker, initializationToken);
    }

    #endregion Public 方法
}