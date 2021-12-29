using System.Runtime.CompilerServices;

using Juxtapose;
using Juxtapose.ObjectPool;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ResourceBasedDynamicObjectPool
{
    public abstract class ResourceBasedObjectPool<T>
        : DynamicObjectPool<T>
        where T : IIllusion
    {
        #region Private 字段

        private static readonly ConditionalWeakTable<object, InstanceUsageInfo> s_instanceReturnTimeTable = new();

        private readonly ILogger _logger;

        private readonly ResourceBasedObjectPoolSchedulerOptions _options;

        #endregion Private 字段

        #region Protected 构造函数

        protected ResourceBasedObjectPool(IDynamicObjectPoolScheduler<T> scheduler,
                                          ILogger logger,
                                          IOptions<ResourceBasedObjectPoolSchedulerOptions> optionsAccessor)
            : base(scheduler)
        {
            _logger = logger;
            _options = optionsAccessor.Value;
        }

        #endregion Protected 构造函数

        #region Protected 方法

        /// <inheritdoc/>
        protected override void Destroy(T instance)
        {
            try
            {
                _logger.LogInformation("Illusion object - {0} Destroying.", instance);
                instance.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Destroy Illusion object - {0} Fail.", instance);
            }
        }

        /// <inheritdoc/>
        protected override void OnSchedulerResourcePressure(ResourcePressureLevel level)
        {
            _logger.LogDebug("Resource Pressure Triggered. Start Contraction Pool Size Check. Level: {0}", level);

            var count = ObjectQueue.Count;

            var targetDestroyCount = level switch
            {
                ResourcePressureLevel.Low => count > 1 ? 1 : 0,
                ResourcePressureLevel.High => count,
                _ => count > 2 ? count / 2 : 1,
            };

            T? first = default;

            var shouldDestroyTime = DateTimeOffset.UtcNow.Add(-_options.DestoryIdleObjectTime);

            for (int i = 0; i < count && targetDestroyCount > 0; i++)
            {
                if (!ObjectQueue.TryDequeue(out var instance))
                {
                    return;
                }

                if (first is null)
                {
                    first = instance;
                }
                else if (ReferenceEquals(first, instance))
                {
                    ObjectQueue.Enqueue(instance);
                    break;
                }

                bool shouldDestroy = !instance.IsAvailable
                                     || !TryGetLastReturnTime(instance, out var lastReturnTime)
                                     || lastReturnTime < shouldDestroyTime;

                if (shouldDestroy)
                {
                    Destroy(instance);
                    Interlocked.Decrement(ref InternalTotalCount);
                    Scheduler.OnDestroyed(instance);
                    targetDestroyCount--;
                }
                else
                {
                    ObjectQueue.Enqueue(instance);
                }
            }
        }

        #endregion Protected 方法

        #region Public 方法

        /// <inheritdoc/>
        public override void Return(T? item)
        {
            SetLastReturnTime(item, DateTimeOffset.UtcNow);
            base.Return(item);
        }

        #endregion Public 方法

        #region UsageInfo

        private static void SetLastReturnTime(T? insatnce, DateTimeOffset lastReturnTime)
        {
            if (insatnce is null)
            {
                return;
            }
            s_instanceReturnTimeTable.AddOrUpdate(insatnce, new() { LastReturnTime = lastReturnTime });
        }

        private static bool TryGetLastReturnTime(T insatnce, out DateTimeOffset lastReturnTime)
        {
            if (s_instanceReturnTimeTable.TryGetValue(insatnce, out var instanceUsageInfo))
            {
                lastReturnTime = instanceUsageInfo.LastReturnTime;
                return true;
            }
            lastReturnTime = default;
            return false;
        }

        private class InstanceUsageInfo
        {
            #region Public 属性

            public DateTimeOffset LastReturnTime { get; set; } = DateTimeOffset.UtcNow;

            #endregion Public 属性
        }

        #endregion UsageInfo
    }
}