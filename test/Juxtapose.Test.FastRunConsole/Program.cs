using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

#pragma warning disable CS8321 // 已声明本地函数，但从未使用过

namespace Juxtapose.Test.FastRunConsole;

internal class Program
{
    #region Private 字段

    private static readonly ILogger s_logger = GreeterJuxtaposeContext.SharedInstance.FastSetConsoleLoggerFactory(LogLevel.Trace).CreateLogger("Juxtapose.Test.FastRunConsole.Program");

    #endregion Private 字段

    #region Private 方法

    private static async Task Main(string[] args)
    {
        s_logger.LogInformation("Current ProcessId: {0}", Environment.ProcessId);

        await TestRunAsync();

        GreeterJuxtaposeContext.SharedInstance.UnSetConsoleLoggerFactory();

        //await TestRunGCAsync(ParallelRunInstanceAsyncMethodAsync);
        //await TestRunGCAsync(ParallelRunStaticAsyncMethodAsync);
        //await TestRunGCAsync(ParallelRunStaticAsyncMethodCancelableAsync);
        //await TestRunGCAsync(ParallelRunInstanceAsyncMethodCancelableAsync);
        //await TestRunGCAsync(ParallelRunInstanceMethodWithFuncAsync);
        //await TestRunGCAsync(ParallelRunStaticMethodWithFuncAsync);

        #region Test

        static async Task ParallelRunInstanceAsyncMethodAsync()
        {
            using var instance = new GreeterAsIGreeterIllusion();

            await ParallelRun(async () =>
            {
                if (string.IsNullOrWhiteSpace(await instance.AsyncMethod("Jerry")))
                {
                    throw new Exception();
                }
            });
        }

        static async Task ParallelRunInstanceMethodWithFuncAsync()
        {
            using var instance = new GreeterAsIGreeterIllusion();

            await ParallelRun(async () =>
            {
                var value = SharedRandom.Shared.Next(1, 1000).ToString();
                string callbackValue = null;
                var funcResult = await instance.MethodWithFuncAsync(v =>
                {
                    callbackValue = v;
                    return Task.FromResult(v);
                }, value);

                if (funcResult != callbackValue)
                {
                    throw new Exception();
                }
            });
        }

        static async Task ParallelRunStaticMethodWithFuncAsync()
        {
            await ParallelRun(async () =>
            {
                var value = SharedRandom.Shared.Next(1, 1000).ToString();
                string callbackValue = null;
                var funcResult = await StaticGreeterIllusion.MethodWithFuncAsync(v =>
                {
                    callbackValue = v;
                    return Task.FromResult(v);
                }, value);

                if (funcResult != callbackValue)
                {
                    throw new Exception();
                }
            });
        }

        static Task ParallelRunStaticAsyncMethodAsync()
        {
            return ParallelRun(async () =>
            {
                if (string.IsNullOrWhiteSpace(await StaticGreeterIllusion.AsyncMethod("Jerry")))
                {
                    throw new Exception();
                }
            });
        }

        static async Task ParallelRunInstanceAsyncMethodCancelableAsync()
        {
            using var instance = new GreeterAsIGreeterIllusion();
            await ParallelRun(async () =>
            {
                var timeout = SharedRandom.Shared.Next(1, 50);
                var cancelTime1 = SharedRandom.Shared.Next(20, 1000);
                var cancelTime2 = SharedRandom.Shared.Next(30, 1000);
                using var cts1 = new CancellationTokenSource(cancelTime1);
                using var cts2 = new CancellationTokenSource(cancelTime2);
                try
                {
                    if (await instance.AsyncMethodCancelable(timeout, cts1.Token, cts2.Token) != timeout)
                    {
                        throw new Exception();
                    }
                }
                catch (OperationCanceledException)
                { }
            }, short.MaxValue >> 2);
        }

        static async Task ParallelRunStaticAsyncMethodCancelableAsync()
        {
            await ParallelRun(async () =>
            {
                var timeout = SharedRandom.Shared.Next(1, 50);
                var cancelTime1 = SharedRandom.Shared.Next(20, 1000);
                var cancelTime2 = SharedRandom.Shared.Next(30, 1000);
                using var cts1 = new CancellationTokenSource(cancelTime1);
                using var cts2 = new CancellationTokenSource(cancelTime2);
                try
                {
                    if (await StaticGreeterIllusion.AsyncMethodCancelable(timeout, cts1.Token, cts2.Token) != timeout)
                    {
                        throw new Exception();
                    }
                }
                catch (OperationCanceledException)
                { }
            }, short.MaxValue >> 2);
        }

        static async Task ParallelRun(Func<Task> func, int count = short.MaxValue)
        {
            var tasks = Enumerable.Range(0, Environment.ProcessorCount).Select(async m =>
            {
                for (int i = 0; i < count; i++)
                {
                    await func();
                }
            });

            await Task.WhenAll(tasks);
        }

        #endregion Test
    }

    private static async Task TestRunAsync()
    {
        s_logger.LogInformation($"Static.AsyncMethod: {await StaticGreeterIllusion.AsyncMethod("李四")}");

        var instance = new GreeterAsIGreeterIllusion("Lilith");

        s_logger.LogInformation("Create Instance Over");

        {
            var callbackResult = await instance.MethodWithFuncAsync(async (value) =>
            {
                await Task.CompletedTask;
                var result = $"{DateTime.Now}: {value}";
                s_logger.LogInformation("Callback Invoked arg: {0} return: {1}", value, result);
                return result;
            }, "Hello");

            s_logger.LogInformation("Instance.MethodWithFuncAsync: {0}", callbackResult);
        }

        {
            var callbackResult = await instance.MethodWithDelegateAsync((GetLongerStringTaskAsync)(async (value, value2) =>
            {
                await Task.CompletedTask;
                var result = $"{DateTime.Now}: {value}+{value2}";
                s_logger.LogInformation("Delegate callback Invoked arg1: {0} arg2: {1} return: {3}", value, value2, result);
                return result;
            }), "Hello");

            s_logger.LogInformation("Instance.MethodWithDelegateAsync: {0}", callbackResult);
        }

        var stopWatch = Stopwatch.StartNew();
        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            var result = await instance.AsyncMethodCancelable(2000, cts.Token);

            stopWatch.Stop();

            s_logger.LogInformation($"Instance.AsyncMethod ct: {result} - time: {stopWatch.ElapsedMilliseconds} ms");
        }
        catch (OperationCanceledException)
        {
            stopWatch.Stop();

            s_logger.LogInformation("Operation Canceled. time: {0} ms", stopWatch.ElapsedMilliseconds);
        }

        stopWatch.Restart();

        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1.8));
            var cts2 = new CancellationTokenSource(TimeSpan.FromSeconds(1.6));

            var result = await instance.AsyncMethodCancelable(2000, cts.Token, cts2.Token);

            stopWatch.Stop();

            s_logger.LogInformation($"Instance.AsyncMethod ct: {result} - time: {stopWatch.ElapsedMilliseconds} ms");
        }
        catch (OperationCanceledException)
        {
            stopWatch.Stop();

            s_logger.LogInformation("Operation Canceled. time: {0} ms", stopWatch.ElapsedMilliseconds);
        }

        s_logger.LogInformation($"Get Instance Prop: {instance.Prop}");

        instance.Prop = "LilithPlus";

        s_logger.LogInformation($"Get Instance Prop after set: {instance.Prop}");

        s_logger.LogInformation($"Instance.Method: {instance.Method("张三")}");

        s_logger.LogInformation($"Instance.AsyncMethod: {await instance.AsyncMethod("李四")}");

        s_logger.LogInformation("End");

        Console.ReadLine();
    }

    private static async Task TestRunGCAsync(Func<Task> func)
    {
        await func();

        s_logger.LogInformation("Done.");

        s_logger.LogInformation("Input q to quit. other to GC.");

        while (Console.ReadLine()?.Trim() != "q")
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            s_logger.LogInformation("GC Completed.");
        }
    }

    #endregion Private 方法

    #region Internal 类

    internal static class SharedRandom
    {
        #region Private 字段

        private static readonly ThreadLocal<Random> s_random = new(() => new(), false);

        #endregion Private 字段

        #region Public 属性

        public static Random Shared => s_random.Value!;

        #endregion Public 属性
    }

    #endregion Internal 类
}
