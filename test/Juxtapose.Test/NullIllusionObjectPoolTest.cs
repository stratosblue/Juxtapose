using System.Threading.Tasks;

using Juxtapose.ObjectPool;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Juxtapose.Test
{
    [TestClass]
    public class NullIllusionObjectPoolTest
    {
        #region Public 方法

        [TestMethod]
        public async Task ShouldNoException()
        {
            var pool = NullIllusionObjectPool<TestClass>.Instance;

            for (int i = 0; i < 3; i++)
            {
                Assert.IsNull(await pool.GetAsync());
                Assert.AreEqual(0, pool.TotalCount);
                Assert.AreEqual(0, pool.IdleCount);
                pool.Return(null);
                pool.Dispose();
            }
        }

        #endregion Public 方法

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