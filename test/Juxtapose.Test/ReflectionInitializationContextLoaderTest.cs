using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Juxtapose.Test;

[TestClass]
public class ReflectionInitializationContextLoaderTest
{
    #region Public 方法

    [TestMethod]
    public void LoadTest()
    {
        var loader = new ReflectionInitializationContextLoader();
        Assert.IsTrue(loader.Contains(GreeterJuxtaposeContext.SharedInstance.Identifier));
        var context = loader.Get(GreeterJuxtaposeContext.SharedInstance.Identifier);
        Assert.AreEqual(GreeterJuxtaposeContext.SharedInstance.GetType(), context.GetType());
        Assert.AreEqual(GreeterJuxtaposeContext.SharedInstance, context);
    }

    #endregion Public 方法
}
