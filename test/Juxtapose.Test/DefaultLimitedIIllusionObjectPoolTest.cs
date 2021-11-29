using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Juxtapose.ObjectPool;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Juxtapose.Test
{
    [TestClass]
    public class DefaultLimitedIIllusionObjectPoolTest
    {
        //TODO 覆盖未测试路径

        #region Public 方法

        [TestMethod]
        public async Task ShouldBlockAtGet()
        {
            var pool = LimitedIIllusionObjectPool.Create(Create, NoCheck, 4, 4, true);
            Assert.IsNotNull(await pool.GetAsync());
            Assert.IsNotNull(await pool.GetAsync());
            Assert.IsNotNull(await pool.GetAsync());
            var last = await pool.GetAsync();
            Assert.IsNotNull(last);

            var random = new Random();
            var interval = random.Next(200, 500);

            var startTime = DateTime.Now;
            var endTime = DateTime.Now;

            TestClass? waitGot = null;
            var rentTask = Task.Run(async () =>
            {
                waitGot = await pool.GetAsync();
                endTime = DateTime.Now;
            });

            var returnTask = Task.Run(async () =>
            {
                await Task.Delay(interval);
                pool.Return(last);
            });

            await Task.WhenAll(rentTask, returnTask);

            Assert.IsTrue((endTime - startTime).TotalMilliseconds >= interval);

            Assert.IsTrue(ReferenceEquals(last, waitGot));
        }

        [TestMethod]
        public async Task ShouldNotBlockAtGet()
        {
            var pool = LimitedIIllusionObjectPool.Create(Create, NoCheck, 4, 4, false);
            Assert.IsNotNull(await pool.GetAsync());
            Assert.IsNotNull(await pool.GetAsync());
            Assert.IsNotNull(await pool.GetAsync());
            var last = await pool.GetAsync();
            Assert.IsNotNull(last);

            var random = new Random();
            var interval = random.Next(200, 500);

            var startTime = DateTime.Now;
            var endTime = DateTime.Now;

            TestClass? waitGot = null;
            var rentTask = Task.Run(async () =>
            {
                waitGot = await pool.GetAsync();
                endTime = DateTime.Now;
            });

            var returnTask = Task.Run(async () =>
            {
                await Task.Delay(interval);
                pool.Return(last);
            });

            await Task.WhenAll(rentTask, returnTask);

            Assert.IsTrue((endTime - startTime).TotalMilliseconds < interval);

            Assert.IsNull(waitGot);
        }

        [TestMethod]
        public async Task ShouldRetainedTargetNumber()
        {
            var random = new Random();
            var retainedNum = random.Next(10, 15);
            var maxNum = retainedNum * 2;

            var pool = LimitedIIllusionObjectPool.Create(Create, NoCheck, retainedNum, maxNum, true);

            var tasks = Enumerable.Range(0, maxNum).Select(async m =>
            {
                var random = new Random();
                var count = random.Next(10, 20);
                for (var i = 0; i < maxNum; i++)
                {
                    var obj = await pool.GetAsync();
                    Assert.IsNotNull(obj);

                    //HACK 随机延时，可能过小，导致未达到最小池大小
                    await Task.Delay(random.Next(20, 30));

                    pool.Return(obj);
                }
            }).ToArray();

            await Task.WhenAll(tasks);

            Assert.AreEqual(retainedNum, pool.CurrentCount);
        }

        [TestMethod]
        public void ShouldThrownOnCreate()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => LimitedIIllusionObjectPool.Create(Create, NoCheck, 4, 3, true));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => LimitedIIllusionObjectPool.Create(Create, NoCheck, 4, 0, true));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => LimitedIIllusionObjectPool.Create(Create, NoCheck, 4, -2, true));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => LimitedIIllusionObjectPool.Create(Create, NoCheck, -1, 3, true));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => LimitedIIllusionObjectPool.Create(Create, NoCheck, -2, 3, true));
            Assert.ThrowsException<ArgumentNullException>(() => LimitedIIllusionObjectPool.Create<TestClass>(null, NoCheck, 4, 4, true));
            Assert.ThrowsException<ArgumentNullException>(() => LimitedIIllusionObjectPool.Create<TestClass>(Create, null, 4, 4, true));
            Assert.ThrowsException<ArgumentNullException>(() => LimitedIIllusionObjectPool.Create<TestClass>(null, null, 4, 4, true));
        }

        #endregion Public 方法

        #region Private 方法

        private static Task<TestClass?> Create(CancellationToken cancellation)
        {
            return Task.FromResult(new TestClass());
        }

        private static bool NoCheck(TestClass item)
        {
            return true;
        }

        #endregion Private 方法

        #region Private 类

        private class TestClass : IIllusion
        {
            #region Public 属性

            public JuxtaposeExecutor Executor { get; }

            public bool IsAvailable { get; private set; } = true;

            #endregion Public 属性

            #region Public 方法

            public void Dispose()
            {
                IsAvailable = false;
            }

            #endregion Public 方法
        }

        #endregion Private 类
    }
}