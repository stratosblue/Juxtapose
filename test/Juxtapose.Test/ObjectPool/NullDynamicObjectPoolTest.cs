using Juxtapose.ObjectPool;

namespace Juxtapose.Test.ObjectPool;

[TestClass]
public class NullDynamicObjectPoolTest
{
    #region Public 方法

    [TestMethod]
    public async Task ShouldNoException()
    {
        var pool = NullDynamicObjectPool<object>.Instance;

        for (int i = 0; i < 5; i++)
        {
            Assert.IsNull(await pool.RentAsync());
            pool.Return(null);
            pool.Dispose();
        }
    }

    #endregion Public 方法
}
