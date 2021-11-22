using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GreeterIllusion = global::Juxtapose.Test.GreeterAsIGreeterIllusion;
using GreeterOrigin = global::Juxtapose.Test.Greeter;

namespace Juxtapose.Test
{
    [TestClass]
    public class GreeterTest
    {
        #region Public 字段

        public const int ParallelRunCount = 100;

        #endregion Public 字段

        #region Private 方法

        private static GreeterIllusion CreateObject(out GreeterOrigin origin)
        {
            origin = new GreeterOrigin("CSharp");
            return new GreeterIllusion("CSharp");
        }

        #endregion Private 方法

        #region Public 方法

        [TestMethod]
        public async Task ShouldCancelSuccessByOneTokenAsync()
        {
            const int CancelTime = 1000;
            const int WaitTime = 2000;

            using var illusion = CreateObject(out var origin);

            await CheckAsync<TaskCanceledException>(token => origin.AsyncMethodCancelable(WaitTime, token));

            await CheckAsync<OperationCanceledException>(token => illusion.AsyncMethodCancelable(WaitTime, token));

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

            using var illusion = CreateObject(out var origin);

            await CheckAsync<TaskCanceledException>((token, token2) => origin.AsyncMethodCancelable(WaitTime, token, token2));

            await CheckAsync<OperationCanceledException>((token, token2) => illusion.AsyncMethodCancelable(WaitTime, token, token2));

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
        public async Task ShouldEqualsDirectInvokeObject()
        {
            const string InputPrefix = "Hello World";

            string input;

            using var illusion = CreateObject(out var origin);

            PropCheck(origin, illusion);

            ReNewInput();
            origin.Prop = input;
            illusion.Prop = input;
            PropCheck(origin, illusion);

            ReNewInput();
            origin.PropSet = input;
            illusion.PropSet = input;
            PropCheck(origin, illusion);

            ReNewInput();
            AssertUtil.Equal(await origin.AsyncMethod(input), await illusion.AsyncMethod(input));

            ReNewInput();
            await origin.AsyncMethodWithoutReturn(input);
            await illusion.AsyncMethodWithoutReturn(input);
            PropCheck(origin, illusion);

            ReNewInput();
            AssertUtil.Equal(await origin.AwaitedAsyncMethod(input), await illusion.AwaitedAsyncMethod(input));

            ReNewInput();
            AssertUtil.Equal(await origin.AwaitedValueTaskAsyncMethod(input), await illusion.AwaitedValueTaskAsyncMethod(input));

            ReNewInput();
            AssertUtil.Equal(origin.Method(input), illusion.Method(input));

            ReNewInput();
            origin.MethodWithoutReturn(input);
            illusion.MethodWithoutReturn(input);
            PropCheck(origin, illusion);

            ReNewInput();
            AssertUtil.Equal(await origin.ValueTaskAsyncMethod(input), await illusion.ValueTaskAsyncMethod(input));

            ReNewInput();
            await origin.ValueTaskAsyncMethodWithoutReturn(input);
            await illusion.ValueTaskAsyncMethodWithoutReturn(input);
            PropCheck(origin, illusion);

            void ReNewInput()
            {
                input = $"{InputPrefix}:{DateTime.UtcNow.Ticks}";
            }
        }

        [TestMethod]
        public void ShouldSuccessParallelInvokeMethod()
        {
            using var illusion = CreateObject(out var origin);
            Parallel.For(0, 100, _ =>
            {
                var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

                Assert.AreEqual(origin.Method(time), illusion.Method(time));
            });
        }

        [TestMethod]
        public async Task ShouldSuccessParallelInvokeMethodAsync()
        {
            using var illusion = CreateObject(out var origin);

            var tasks = Enumerable.Range(0, ParallelRunCount * 100).Select(async m =>
            {
                var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

                Assert.AreEqual(await origin.AsyncMethod(time), await illusion.AsyncMethod(time));
            }).ToArray();
            await Task.WhenAll(tasks);
        }

        #endregion Public 方法

        #region Protected 方法

        protected static void PropCheck(GreeterOrigin origin, GreeterIllusion illusion)
        {
            AssertUtil.Equal(origin.Prop, illusion.Prop);
            AssertUtil.Equal(origin.PropGet, illusion.PropGet);
        }

        #endregion Protected 方法
    }
}