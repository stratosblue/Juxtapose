using Juxtapose;
using Juxtapose.ObjectPool;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ResourceBasedDynamicObjectPool;

public class ResourceBasedObjectPoolScheduler : DynamicObjectPoolScheduler<IIllusion>
{
    #region Private 字段

    private readonly ILogger _logger;

    private readonly ResourceBasedObjectPoolSchedulerOptions _options;

    /// <summary>
    /// 释放进程的内存大小阈值
    /// </summary>
    private readonly StorageSize _processDisposeMemoryThreshold;

    private int _totalCount = 0;

    #endregion Private 字段

    #region Public 构造函数

    public ResourceBasedObjectPoolScheduler(IOptions<ResourceBasedObjectPoolSchedulerOptions> optionsAccessor, ILogger<ResourceBasedObjectPoolScheduler> logger)
    {
        _options = optionsAccessor.Value;

        var processCount = _options.ProcessCountBaseLine > 1 ? _options.ProcessCountBaseLine : 2;
        _processDisposeMemoryThreshold = processCount > 2
                                         ? SystemInfo.MemoryInfo.Total / processCount
                                         : SystemInfo.MemoryInfo.Total / 2;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("Total Memory: {TotalMemory} , Available: {AvailableMemory} , Threshold: {MemoryThreshold} , CountBaseLine: {BaseLine}.", SystemInfo.MemoryInfo.Total, SystemInfo.MemoryInfo.Available, _processDisposeMemoryThreshold, _options.ProcessCountBaseLine);

        _ = AutoContractionAsync(RunningToken);
    }

    #endregion Public 构造函数

    #region Protected 方法

    /// <summary>
    /// 自动收缩
    /// </summary>
    /// <param name="loopToken">停止处理循环token</param>
    protected virtual async Task AutoContractionAsync(CancellationToken loopToken)
    {
        while (!loopToken.IsCancellationRequested)
        {
            await Task.Delay(_options.AutoContractionInterval, loopToken);
            _logger.LogDebug("Trigger Resource Pressure Event.");
            try
            {
                TriggerResourcePressure(ResourcePressureLevel.Low);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trigger Resource Pressure Error.");
            }
        }
    }

    protected bool GetCanCreate()
    {
        return _totalCount < _options.ProcessCountBaseLine / 2
               || _totalCount < _options.ProcessCountBaseLine
                   && SystemInfo.MemoryInfo.Available > _processDisposeMemoryThreshold;
    }

    #endregion Protected 方法

    #region Public 方法

    public override ValueTask<bool> CanCreateAsync(CancellationToken cancellation = default)
    {
        var canCreate = GetCanCreate();

        if (!canCreate)
        {
            //释放一次资源再做判断
            TriggerResourcePressure(ResourcePressureLevel.High);
            canCreate = GetCanCreate();
        }

        return ValueTask.FromResult(canCreate);
    }

    public override void OnCreated(IIllusion instance)
    {
        Interlocked.Increment(ref _totalCount);
    }

    public override void OnDestroyed(IIllusion instance)
    {
        Interlocked.Decrement(ref _totalCount);
    }

    public override bool OnReturn(IIllusion instance)
    {
        if (!instance.TryGetExternalProcess(out var externalProcess))
        {
            _logger.LogInformation("Illusion object {Instance} should destory because of can not get Process.", instance);
            return false;
        }
        if (!externalProcess.IsAlive)
        {
            _logger.LogInformation("Illusion object {Instance} should destory because of Process is not alive.", instance);
            return false;
        }
        if (externalProcess.GetMemoryUsage() is not long memoryUsage
            || memoryUsage < 1)
        {
            _logger.LogInformation("Illusion object {Instance} should destory because of can not get MemoryUsage.", instance);
            return false;
        }

        var memorySize = new StorageSize(memoryUsage);

        if (memorySize > _processDisposeMemoryThreshold
            || SystemInfo.MemoryInfo.Available < _processDisposeMemoryThreshold)
        {
            _logger.LogInformation("Illusion object {Instance} should destory because of memory usage. Used: {MemorySize} , Threshold: {Threshold}", instance, memorySize, _processDisposeMemoryThreshold);
            return false;
        }

        var totalCount = _totalCount;

        if (totalCount > _options.ProcessCountBaseLine)
        {
            _logger.LogInformation("Illusion object {Instance} should destory because of process count larger than BaseLine. Current: {TotalCount} , BaseLine: {BaseLine}", instance, totalCount, _options.ProcessCountBaseLine);
            return false;
        }

        return true;
    }

    public override string ToString()
    {
        return $"Current Total Count: {_totalCount} . Total Memory: {SystemInfo.MemoryInfo.Total} , Available: {SystemInfo.MemoryInfo.Available} , Threshold: {_processDisposeMemoryThreshold} , CountBaseLine: {_options.ProcessCountBaseLine} .";
    }

    #endregion Public 方法
}
