using StaticGreeterOrigin = global::Juxtapose.Test.StaticGreeter;

namespace Juxtapose.Test;

[TestClass]
public class StaticGreeterCallbackTest
{
    #region Private 属性

    private static string Input => Guid.NewGuid().ToString();

    #endregion Private 属性

    #region Public 方法

    [TestMethod]
    public void ShouldWorkWithAction()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(StaticGreeterOrigin.MethodWithAction(() => originCallbackValue = "1", input), StaticGreeterIllusion.MethodWithAction(() => illusionCallbackValue = "1", input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public void ShouldWorkWithActionT()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(StaticGreeterOrigin.MethodWithAction(value => originCallbackValue = value, input), StaticGreeterIllusion.MethodWithAction(value => illusionCallbackValue = value, input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public void ShouldWorkWithActionTT()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(StaticGreeterOrigin.MethodWithAction((value, value2) => originCallbackValue = value + value2, input), StaticGreeterIllusion.MethodWithAction((value, value2) => illusionCallbackValue = value + value2, input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public void ShouldWorkWithDelegate()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(StaticGreeterOrigin.MethodWithDelegate((value, value2) => originCallbackValue = value + value2, input), StaticGreeterIllusion.MethodWithDelegate((value, value2) => illusionCallbackValue = value + value2, input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public async Task ShouldWorkWithDelegateTaskAsync()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(await StaticGreeterOrigin.MethodWithDelegateAsync((value, value2) => Task.FromResult(originCallbackValue = value + value2), input), await StaticGreeterIllusion.MethodWithDelegateAsync((value, value2) => Task.FromResult(illusionCallbackValue = value + value2), input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public async Task ShouldWorkWithDelegateVeluaTaskAsync()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(await StaticGreeterOrigin.MethodWithDelegateAsync((value, value2) => new ValueTask<string>(originCallbackValue = value + value2), input), await StaticGreeterIllusion.MethodWithDelegateAsync((value, value2) => new ValueTask<string>(illusionCallbackValue = value + value2), input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public void ShouldWorkWithFunc()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(StaticGreeterOrigin.MethodWithFunc(value => originCallbackValue = value, input), StaticGreeterIllusion.MethodWithFunc(value => illusionCallbackValue = value, input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public async Task ShouldWorkWithFuncTAsync()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(await StaticGreeterOrigin.MethodWithFuncAsync(value => Task.FromResult(originCallbackValue = value), input), await StaticGreeterIllusion.MethodWithFuncAsync(value => Task.FromResult(illusionCallbackValue = value), input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public async Task ShouldWorkWithFuncTTAsync()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(await StaticGreeterOrigin.MethodWithFuncAsync((value, value2) => Task.FromResult(originCallbackValue = value + value2), input), await StaticGreeterIllusion.MethodWithFuncAsync((value, value2) => Task.FromResult(illusionCallbackValue = value + value2), input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    #endregion Public 方法
}
