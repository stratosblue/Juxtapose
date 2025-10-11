namespace Juxtapose.Test;

[TestClass]
public class JuxtaposeEnvironmentTest
{
    #region Public 方法

    [TestMethod]
    public void ShouldCheckIsSubProcessSuccess()
    {
        Assert.IsFalse(JuxtaposeEnvironmentProxy.IsSubProcess);
        Assert.IsTrue(ExternalJuxtaposeEnvironment.IsSubProcess);
    }

    #endregion Public 方法
}
