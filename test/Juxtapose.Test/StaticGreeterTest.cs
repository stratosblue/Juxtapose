using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using StaticGreeterOrigin = global::Juxtapose.Test.StaticGreeter;

namespace Juxtapose.Test;

[TestClass]
public class StaticGreeterTest
{
    #region Public 字段

    public const int ParallelRunCount = 100;

    #endregion Public 字段

    #region Public 方法

    [TestMethod]
    public async Task ShouldCancelSuccessByOneTokenAsync()
    {
        const int CancelTime = 1000;
        const int WaitTime = 2000;

        await CheckAsync<TaskCanceledException>(token => StaticGreeterOrigin.AsyncMethodCancelable(WaitTime, token));

        await CheckAsync<OperationCanceledException>(token => StaticGreeterIllusion.AsyncMethodCancelable(WaitTime, token));

        async static Task CheckAsync<TException>(Func<CancellationToken, Task> action) where TException : Exception
        {
            var stopwatch = Stopwatch.StartNew();
            using var cts = new CancellationTokenSource(CancelTime);

            await Assert.ThrowsExceptionAsync<TException>(() => action(cts.Token));

            stopwatch.Stop();

            Debug.WriteLine("ElapsedMilliseconds: {0}", stopwatch.ElapsedMilliseconds);
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= CancelTime - 20 && stopwatch.ElapsedMilliseconds <= WaitTime);
        }
    }

    [TestMethod]
    public async Task ShouldCancelSuccessByTwoTokenAsync()
    {
        const int CancelTime = 1000;
        const int CancelTime2 = 1500;
        const int WaitTime = 2000;

        await CheckAsync<TaskCanceledException>((token, token2) => StaticGreeterOrigin.AsyncMethodCancelable(WaitTime, token, token2));

        await CheckAsync<OperationCanceledException>((token, token2) => StaticGreeterIllusion.AsyncMethodCancelable(WaitTime, token, token2));

        async static Task CheckAsync<TException>(Func<CancellationToken, CancellationToken, Task> action) where TException : Exception
        {
            var stopwatch = Stopwatch.StartNew();
            using var cts = new CancellationTokenSource(CancelTime2);
            using var cts2 = new CancellationTokenSource(CancelTime);

            await Assert.ThrowsExceptionAsync<TException>(() => action(cts.Token, cts2.Token));

            stopwatch.Stop();

            Debug.WriteLine("ElapsedMilliseconds: {0}", stopwatch.ElapsedMilliseconds);
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= CancelTime - 20 && stopwatch.ElapsedMilliseconds <= CancelTime2);
        }
    }

    [TestMethod]
    public async Task ShouldEqualsDirectInvoke()
    {
        const string InputPrefix = "Hello World";

        string input;

        PropCheck();

        ReNewInput();
        StaticGreeterOrigin.Prop = input;
        StaticGreeterIllusion.Prop = input;
        PropCheck();

        ReNewInput();
        StaticGreeterOrigin.PropSet = input;
        StaticGreeterIllusion.PropSet = input;
        PropCheck();

        ReNewInput();
        AssertUtil.Equal(await StaticGreeterOrigin.AsyncMethod(input), await StaticGreeterIllusion.AsyncMethod(input));

        var arrayInput = new int[] { 1, 2, 3 };
        var arrayOutput = await StaticGreeterOrigin.AsyncMethod(arrayInput);
        var arrayOutput1 = await StaticGreeterIllusion.AsyncMethod(arrayInput);
        AssertUtil.Equal(arrayOutput.Length, arrayOutput1.Length);
        for (int i = 0; i < arrayOutput.Length; i++)
        {
            AssertUtil.Equal(arrayOutput[i], arrayOutput1[i]);
        }

        int? nullableIntInput = 1;
        AssertUtil.Equal(await StaticGreeterOrigin.AsyncMethod(nullableIntInput), await StaticGreeterIllusion.AsyncMethod(nullableIntInput));

        ReNewInput();
        await StaticGreeterOrigin.AsyncMethodWithoutReturn(input);
        await StaticGreeterIllusion.AsyncMethodWithoutReturn(input);
        PropCheck();

        ReNewInput();
        AssertUtil.Equal(await StaticGreeterOrigin.AwaitedAsyncMethod(input), await StaticGreeterIllusion.AwaitedAsyncMethod(input));

        ReNewInput();
        AssertUtil.Equal(await StaticGreeterOrigin.AwaitedValueTaskAsyncMethod(input), await StaticGreeterIllusion.AwaitedValueTaskAsyncMethod(input));

        ReNewInput();
        AssertUtil.Equal(StaticGreeterOrigin.Method(input), StaticGreeterIllusion.Method(input));

        ReNewInput();
        StaticGreeterOrigin.MethodWithoutReturn(input);
        StaticGreeterIllusion.MethodWithoutReturn(input);
        PropCheck();

        ReNewInput();
        AssertUtil.Equal(await StaticGreeterOrigin.ValueTaskAsyncMethod(input), await StaticGreeterIllusion.ValueTaskAsyncMethod(input));

        ReNewInput();
        await StaticGreeterOrigin.ValueTaskAsyncMethodWithoutReturn(input);
        await StaticGreeterIllusion.ValueTaskAsyncMethodWithoutReturn(input);
        PropCheck();

        void ReNewInput()
        {
            input = $"{InputPrefix}:{DateTime.UtcNow.Ticks}";
        }
    }

    [TestMethod]
    public void ShouldHasDefaultValue()
    {
        Assert.AreEqual(StaticGreeterOrigin.MethodWithDefaultValue(), StaticGreeterIllusion.MethodWithDefaultValue());
    }

    [TestMethod]
    public void ShouldSuccessParallelInvokeMethod()
    {
        Parallel.For(0, 100, _ =>
        {
            var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            Assert.AreEqual(StaticGreeterOrigin.Method(time), StaticGreeterIllusion.Method(time));
        });
    }

    [TestMethod]
    public async Task ShouldSuccessParallelInvokeMethodAsync()
    {
        var tasks = Enumerable.Range(0, ParallelRunCount * 100).Select(async m =>
        {
            var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            Assert.AreEqual(await StaticGreeterOrigin.AsyncMethod(time), await StaticGreeterIllusion.AsyncMethod(time));
        }).ToArray();
        await Task.WhenAll(tasks);
    }

    #endregion Public 方法

    #region Protected 方法

    protected static void PropCheck()
    {
        AssertUtil.Equal(StaticGreeterOrigin.Prop, StaticGreeterIllusion.Prop);
        AssertUtil.Equal(StaticGreeterOrigin.PropGet, StaticGreeterIllusion.PropGet);
    }

    #endregion Protected 方法
}
