using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using GreeterOrigin = global::Juxtapose.Test.Greeter;

namespace Juxtapose.Test;

[TestClass]
public class GreeterCallbackTest
{
    #region Private 属性

    private GreeterIllusion _illusion;
    private GreeterOrigin _origin;
    private static string Input => Guid.NewGuid().ToString();

    #endregion Private 属性

    #region Public 方法

    [TestCleanup]
    public void Cleanup()
    {
        _illusion.Dispose();
        _origin = null;
        _illusion = null;
    }

    [TestInitialize]
    public async Task Init()
    {
        _origin = new GreeterOrigin("CSharp");
        _illusion = await GreeterIllusion.NewAsync("CSharp");
    }

    [TestMethod]
    public void ShouldWorkWithAction()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(_origin.MethodWithAction(() => originCallbackValue = "1", input), _illusion.MethodWithAction(() => illusionCallbackValue = "1", input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public void ShouldWorkWithActionT()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(_origin.MethodWithAction(value => originCallbackValue = value, input), _illusion.MethodWithAction(value => illusionCallbackValue = value, input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public void ShouldWorkWithActionTT()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(_origin.MethodWithAction((value, value2) => originCallbackValue = value + value2, input), _illusion.MethodWithAction((value, value2) => illusionCallbackValue = value + value2, input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public void ShouldWorkWithDelegate()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(_origin.MethodWithDelegate((value, value2) => originCallbackValue = value + value2, input), _illusion.MethodWithDelegate((value, value2) => illusionCallbackValue = value + value2, input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public async Task ShouldWorkWithDelegateTaskAsync()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(await _origin.MethodWithDelegateAsync((value, value2) => Task.FromResult(originCallbackValue = value + value2), input), await _illusion.MethodWithDelegateAsync((value, value2) => Task.FromResult(illusionCallbackValue = value + value2), input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public async Task ShouldWorkWithDelegateVeluaTaskAsync()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(await _origin.MethodWithDelegateAsync((value, value2) => new ValueTask<string>(originCallbackValue = value + value2), input), await _illusion.MethodWithDelegateAsync((value, value2) => new ValueTask<string>(illusionCallbackValue = value + value2), input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public void ShouldWorkWithFunc()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(_origin.MethodWithFunc(value => originCallbackValue = value, input), _illusion.MethodWithFunc(value => illusionCallbackValue = value, input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public async Task ShouldWorkWithFuncTAsync()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(await _origin.MethodWithFuncAsync(value => Task.FromResult(originCallbackValue = value), input), await _illusion.MethodWithFuncAsync(value => Task.FromResult(illusionCallbackValue = value), input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    [TestMethod]
    public async Task ShouldWorkWithFuncTTAsync()
    {
        var input = Input;

        string originCallbackValue = null;
        string illusionCallbackValue = null;

        AssertUtil.Equal(await _origin.MethodWithFuncAsync((value, value2) => Task.FromResult(originCallbackValue = value + value2), input), await _illusion.MethodWithFuncAsync((value, value2) => Task.FromResult(illusionCallbackValue = value + value2), input));

        Assert.AreEqual(originCallbackValue, illusionCallbackValue);
    }

    #endregion Public 方法
}
