using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace Juxtapose;

/// <inheritdoc cref="IJuxtaposeExecutorPool"/>
public class JuxtaposeExecutorPool(IInitializationContext context,
                                   IExecutorPoolPolicy poolPolicy,
                                   ILoggerFactory loggerFactory)
    : IJuxtaposeExecutorPool
{
    #region Private 字段

    private readonly IInitializationContext _context = context ?? throw new ArgumentNullException(nameof(context));

    private readonly SemaphoreSlim _executorCreateSemaphore = new(1, 1);

    private readonly ILogger _logger = loggerFactory.CreateLogger("Juxtapose.JuxtaposeExecutorPool");

    private bool _isDisposed;

    #endregion Private 字段

    #region Protected 属性

    /// <summary>
    /// 执行器持有列表
    /// </summary>
    protected ConcurrentDictionary<string, IJuxtaposeExecutorHolder> ExecutorHolders { get; } = new();

    /// <inheritdoc cref="IExecutorPoolPolicy"/>
    protected IExecutorPoolPolicy Policy { get; } = poolPolicy ?? throw new ArgumentNullException(nameof(poolPolicy));

    #endregion Protected 属性

    #region Private 方法

    private async Task<IJuxtaposeExecutorHolder> GetAndHoldExecutorHolderAsync(ExecutorCreationContext creationContext,
                                                                               string identifier,
                                                                               int? holdLimit,
                                                                               CancellationToken cancellation,
                                                                               bool shouldRetry = true)
    {
        cancellation.ThrowIfCancellationRequested();

        var holder = await TryGetExecutorHolderAsync(creationContext, identifier, holdLimit, cancellation);

        //HACK 新的Holder是否需要循环检查
        if (holder.IsDisposed
            || holder.Executor.IsDisposed)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("Executor holder {Identifier} has disposed. Try get a new holder.", identifier);
            }
            RemoveExecutorHolder(identifier);
            holder = await TryGetExecutorHolderAsync(creationContext, identifier, holdLimit, cancellation);
        }

        if (holder.IsDisposed
            || holder.Executor.IsDisposed)
        {
            throw new JuxtaposeException($"Create executor holder [{identifier}] fail");
        }

        try
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Holding executor - {Identifier} .", identifier);
            }

            await holder.HoldAsync(cancellation);

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Executor Holded - {Identifier} .", identifier);
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            if (shouldRetry
                && (holder.IsDisposed || holder.Executor.IsDisposed))
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(ex, "Exception threw while holding executor - {Identifier} . And holder has disposed. Try get a executor again.", identifier);
                }

                return await GetAndHoldExecutorHolderAsync(creationContext, identifier, holdLimit, cancellation, false);
            }
            else
            {
                throw;
            }
        }

        return holder;
    }

    #endregion Private 方法

    #region Protected 方法

    /// <summary>
    /// 创建 <see cref="JuxtaposeExecutor"/>
    /// </summary>
    /// <param name="creationContext"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    protected virtual async Task<JuxtaposeExecutor> CreateExecutorAsync(ExecutorCreationContext creationContext, CancellationToken cancellation)
    {
        if (creationContext.IsHosting)
        {
            var bootstrapper = _context.GetBootstrapper();
            return await bootstrapper.PrepareHostingExecutorAsync(cancellation);
        }
        else
        {
            var bootstrapper = _context.GetBootstrapper();

            var options = creationContext.Options
                          ?? throw new JuxtaposeException($"{nameof(ExecutorCreationContext)}.{nameof(ExecutorCreationContext.Options)} can not be null at here.");

            return await bootstrapper.PrepareExecutorAsync(options, cancellation);
        }
    }

    /// <summary>
    /// 创建 <see cref="IJuxtaposeExecutorHolder"/>
    /// </summary>
    /// <param name="executor"></param>
    /// <param name="holdLimit">持有数量限制</param>
    /// <returns></returns>
    protected virtual IJuxtaposeExecutorHolder CreateExecutorHolder(JuxtaposeExecutor executor, int? holdLimit)
    {
        return holdLimit is null
               ? new NoLimitJuxtaposeExecutorHolder(executor)
               : new LimitedJuxtaposeExecutorHolder(executor, holdLimit.Value);
    }

    /// <summary>
    /// 创建 <see cref="IJuxtaposeExecutorOwner"/>
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="executorHolder"></param>
    /// <param name="creationContext"></param>
    /// <returns></returns>
    protected virtual IJuxtaposeExecutorOwner CreateExecutorOwner(string identifier, IJuxtaposeExecutorHolder executorHolder, ExecutorCreationContext creationContext)
    {
        return new JuxtaposeExecutorOwner(identifier, executorHolder, creationContext, Policy, RemoveExecutorHolder);
    }

    /// <summary>
    /// 移除<see cref="IJuxtaposeExecutorHolder"/>
    /// </summary>
    /// <param name="identifier"></param>
    protected void RemoveExecutorHolder(string identifier)
    {
        if (ExecutorHolders.TryRemove(identifier, out var executorHolder))
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Executor {Identifier} prepare drop starting. Current hold count: {HoldCount}.", identifier, executorHolder.Count);
            }

            executorHolder.PrepareDrop();

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Executor {Identifier} prepare drop complete. Is disposed: {IsDisposed} ", identifier, executorHolder.IsDisposed);
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="creationContext"></param>
    /// <param name="identifier"></param>
    /// <param name="holdLimit"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    protected async Task<IJuxtaposeExecutorHolder> TryGetExecutorHolderAsync(ExecutorCreationContext creationContext,
                                                                             string identifier,
                                                                             int? holdLimit,
                                                                             CancellationToken cancellation)
    {
        if (!ExecutorHolders.TryGetValue(identifier, out var holder))
        {
            await _executorCreateSemaphore.WaitAsync(cancellation);
            try
            {
                if (!ExecutorHolders.TryGetValue(identifier, out holder))
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Creating new executor for identifier - {Identifier} .", identifier);
                    }

                    var newExecutor = await CreateExecutorAsync(creationContext, cancellation);
                    holder = CreateExecutorHolder(newExecutor, holdLimit);
                    ExecutorHolders.TryAdd(identifier, holder);
                }
            }
            finally
            {
                _executorCreateSemaphore.Release();
            }
        }

        return holder;
    }

    #endregion Protected 方法

    #region Public 方法

    #region IDisposable

    /// <summary>
    ///
    /// </summary>
    ~JuxtaposeExecutorPool()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc cref="Dispose()"/>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _executorCreateSemaphore.Dispose();
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
    public async Task<IJuxtaposeExecutorOwner> GetAsync(ExecutorCreationContext creationContext, CancellationToken cancellation)
    {
        var identifier = Policy.Classify(creationContext, out var holdLimit);

        var holder = await GetAndHoldExecutorHolderAsync(creationContext, identifier, holdLimit, cancellation, true);

        return CreateExecutorOwner(identifier, holder, creationContext);
    }

    #endregion Public 方法
}
