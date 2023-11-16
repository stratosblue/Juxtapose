using System.Threading;
using System.Threading.Tasks;

using Juxtapose.ObjectPool;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Juxtapose.Test.ObjectPool;

[TestClass]
public class NullDynamicObjectPoolSchedulerTest
{
    #region Public 方法

    [TestMethod]
    public async Task ShouldNoException()
    {
        var scheduler = NullDynamicObjectPoolScheduler<object>.Instance;

        for (int i = 0; i < 5; i++)
        {
            scheduler.ReleaseLock();
            scheduler.OnCreated(i);
            scheduler.OnDestroyed(i);
            scheduler.OnRent(i);
            scheduler.OnReturn(i);
            scheduler.Dispose();
            Assert.IsFalse(await scheduler.CanCreateAsync(default));
            await scheduler.LockAsync(CancellationToken.None);
            scheduler.OnResourcePressure -= SchedulerOnResleveourcePressure;
            scheduler.OnResourcePressure += SchedulerOnResleveourcePressure;
        }

        static void SchedulerOnResleveourcePressure(ResourcePressureLevel level)
        {
            throw new System.NotImplementedException();
        }
    }

    #endregion Public 方法
}
