using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Juxtapose.Utils;

namespace Juxtapose;

/// <summary>
/// 外部进程入口点
/// </summary>
public static class JuxtaposeEntryPoint
{
    #region Public 方法

    #region AsEndpointAsync

    /// <summary>
    /// <inheritdoc cref="TryAsEndpointAsync(string[], IInitializationContext[])"/>
    /// <para/>
    /// 如果启动参数不包含子进程运行信息，则以错误码<see cref="ExternalProcessExitCodes.NoJuxtaposeCommandLineArguments"/>退出进程
    /// </summary>
    /// <param name="args"></param>
    /// <param name="contexts"></param>
    /// <returns></returns>
    public static async Task AsEndpointAsync(string[] args, params IInitializationContext[] contexts)
    {
        var runTask = contexts is null || contexts.Length is 0
                      ? TryRunAsync(args, new ReflectionInitializationContextLoader())
                      : TryRunAsync(args, contexts);

        if (!await runTask)
        {
            ExitWithNoJuxtaposeCommandLineArguments();
        }
    }

    /// <summary>
    /// <inheritdoc cref="TryAsEndpointAsync(string[], Func{IInitializationContext})"/>
    /// <para/>
    /// 如果启动参数不包含子进程运行信息，则以错误码<see cref="ExternalProcessExitCodes.NoJuxtaposeCommandLineArguments"/>退出进程
    /// </summary>
    /// <param name="args"></param>
    /// <param name="contextLoadAction"></param>
    /// <returns></returns>
    public static async Task AsEndpointAsync(string[] args, Func<IInitializationContext> contextLoadAction)
    {
        if (!await TryRunAsync(args, () => new[] { contextLoadAction() }))
        {
            ExitWithNoJuxtaposeCommandLineArguments();
        }
    }

    /// <summary>
    /// <inheritdoc cref="TryAsEndpointAsync(string[], Func{IEnumerable{IInitializationContext}})"/>
    /// <para/>
    /// 如果启动参数不包含子进程运行信息，则以错误码<see cref="ExternalProcessExitCodes.NoJuxtaposeCommandLineArguments"/>退出进程
    /// </summary>
    /// <param name="args"></param>
    /// <param name="contextLoadAction"></param>
    /// <returns></returns>
    public static async Task AsEndpointAsync(string[] args, Func<IEnumerable<IInitializationContext>> contextLoadAction)
    {
        if (!await TryRunAsync(args, contextLoadAction))
        {
            ExitWithNoJuxtaposeCommandLineArguments();
        }
    }

    /// <summary>
    /// <inheritdoc cref="TryAsEndpointAsync(string[], IInitializationContextLoader)"/>
    /// <para/>
    /// 如果启动参数不包含子进程运行信息，则以错误码<see cref="ExternalProcessExitCodes.NoJuxtaposeCommandLineArguments"/>退出进程
    /// </summary>
    /// <param name="args"></param>
    /// <param name="contextLoader"></param>
    /// <returns></returns>
    public static async Task AsEndpointAsync(string[] args, IInitializationContextLoader contextLoader)
    {
        if (!await TryRunAsync(args, contextLoader))
        {
            ExitWithNoJuxtaposeCommandLineArguments();
        }
    }

    #endregion AsEndpointAsync

    #region TryAsEndpointAsync

    /// <summary>
    /// 如果 <paramref name="args"/> 包含子进程运行参数，则加载 <paramref name="contexts"/>，并将当前进程作为子进程终结点运行<para/>
    /// 运行结束后将退出程序（执行<see cref="Environment.Exit"/>）
    /// </summary>
    /// <param name="args"></param>
    /// <param name="contexts"></param>
    /// <returns></returns>
    public static async Task TryAsEndpointAsync(string[] args, params IInitializationContext[] contexts)
    {
        if (await TryRunAsync(args, contexts))
        {
            ExitWithSuccessRunAsEndpoint();
        }
    }

    /// <summary>
    /// 如果 <paramref name="args"/> 包含子进程运行参数，则加载 <paramref name="contextLoadAction"/>，并将当前进程作为子进程终结点运行<para/>
    /// 运行结束后将退出程序（执行<see cref="Environment.Exit"/>）
    /// </summary>
    /// <param name="args"></param>
    /// <param name="contextLoadAction"></param>
    /// <returns></returns>
    public static async Task TryAsEndpointAsync(string[] args, Func<IInitializationContext> contextLoadAction)
    {
        if (await TryRunAsync(args, () => new[] { contextLoadAction() }))
        {
            ExitWithSuccessRunAsEndpoint();
        }
    }

    /// <summary>
    /// 如果 <paramref name="args"/> 包含子进程运行参数，则加载 <paramref name="contextLoadAction"/>，并将当前进程作为子进程终结点运行<para/>
    /// 运行结束后将退出程序（执行<see cref="Environment.Exit"/>）
    /// </summary>
    /// <param name="args"></param>
    /// <param name="contextLoadAction"></param>
    /// <returns></returns>
    public static async Task TryAsEndpointAsync(string[] args, Func<IEnumerable<IInitializationContext>> contextLoadAction)
    {
        if (await TryRunAsync(args, contextLoadAction))
        {
            ExitWithSuccessRunAsEndpoint();
        }
    }

    /// <summary>
    /// 如果 <paramref name="args"/> 包含子进程运行参数，则加载 <paramref name="contextLoader"/>，并将当前进程作为子进程终结点运行<para/>
    /// 运行结束后将退出程序（执行<see cref="Environment.Exit"/>）
    /// </summary>
    /// <param name="args"></param>
    /// <param name="contextLoader"></param>
    /// <returns></returns>
    public static async Task TryAsEndpointAsync(string[] args, IInitializationContextLoader contextLoader)
    {
        if (await TryRunAsync(args, contextLoader))
        {
            ExitWithSuccessRunAsEndpoint();
        }
    }

    #endregion TryAsEndpointAsync

    #region TryRunAsync

    /// <summary>
    /// 如果 <paramref name="args"/> 包含子进程运行参数，则加载 <paramref name="contexts"/>，并阻塞当前线程，运行子进程任务<para/>
    /// </summary>
    /// <param name="args"></param>
    /// <param name="contexts"></param>
    /// <returns>是否已作为子进程运行</returns>
    public static Task<bool> TryRunAsync(string[] args, params IInitializationContext[] contexts)
    {
        return contexts is null || contexts.Length is 0
               ? TryRunAsync(args, new ReflectionInitializationContextLoader())
               : TryRunAsync(args, InitializationContextLoader.Create(contexts));
    }

    /// <summary>
    /// 如果 <paramref name="args"/> 包含子进程运行参数，则加载 <paramref name="contextLoadAction"/>，并阻塞当前线程，运行子进程任务<para/>
    /// </summary>
    /// <param name="args"></param>
    /// <param name="contextLoadAction"></param>
    /// <returns>是否已作为子进程运行</returns>
    public static Task<bool> TryRunAsync(string[] args, Func<IEnumerable<IInitializationContext>> contextLoadAction)
    {
        return TryRunAsync(args, InitializationContextLoader.Create(contextLoadAction));
    }

    /// <summary>
    /// 如果 <paramref name="args"/> 包含子进程运行参数，则加载 <paramref name="contextLoader"/>，并阻塞当前线程，运行子进程任务<para/>
    /// </summary>
    /// <param name="args"></param>
    /// <param name="contextLoader"></param>
    /// <returns>是否已作为子进程运行</returns>
    public static async Task<bool> TryRunAsync(string[] args, IInitializationContextLoader contextLoader)
    {
        if (contextLoader is null)
        {
            throw new ArgumentNullException(nameof(contextLoader));
        }

        if (ExternalProcessArgumentUtil.TryGetJuxtaposeOptions(args, out var startupOptions))
        {
            if (startupOptions.Version < Constants.Version)
            {
                Console.Error.WriteLine($"Juxtapose Version Not Match. Input value is 【{startupOptions.Version}】, Context value is {Constants.Version}.");
                Environment.Exit((int)ExternalProcessExitCodes.JuxtaposeVersionNotMatch);
            }

            if (string.IsNullOrWhiteSpace(startupOptions.ContextIdentifier))
            {
                Console.Error.WriteLine($"Startup Options Required Field Missing - {nameof(startupOptions.ContextIdentifier)}");
                Environment.Exit((int)ExternalProcessExitCodes.StartupOptionsRequiredFieldMissing);
            }

            if (startupOptions.Version == 0)
            {
                Console.Error.WriteLine($"Startup Options Required Field Missing - {nameof(startupOptions.Version)}");
                Environment.Exit((int)ExternalProcessExitCodes.StartupOptionsRequiredFieldMissing);
            }

            if (!contextLoader.Contains(startupOptions.ContextIdentifier))
            {
                Console.Error.WriteLine($"Initialization Context Not Found - {startupOptions.ContextIdentifier}");
                Environment.Exit((int)ExternalProcessExitCodes.InitializationContextNotFound);
            }

            if (startupOptions.ParentProcessId is int parentProcessId
                && parentProcessId > 0)
            {
                WaitParentProcessExit(parentProcessId);
            }

            JuxtaposeEnvironment.IsSubProcess = true;

            var context = contextLoader.Get(startupOptions.ContextIdentifier);
            try
            {
                var executorCreationContext = new ExecutorCreationContext(typeof(JuxtaposeEntryPoint), nameof(TryRunAsync), true, false, startupOptions);
                using var executorOwner = await context.GetExecutorOwnerAsync(executorCreationContext, default);

                try
                {
                    //HACK 调整结束逻辑
                    await Task.Delay(Timeout.Infinite, executorOwner.Executor.RunningToken);
                }
                catch { }
            }
            finally
            {
                if (context is IAsyncDisposable asyncDisposableContext)
                {
                    await asyncDisposableContext.DisposeAsync();
                }
                else if (context is IDisposable disposableContext)
                {
                    disposableContext.Dispose();
                }
            }
            return true;
        }

        return false;
    }

    #endregion TryRunAsync

    #endregion Public 方法

    #region Private 方法

    private static void ExitWithNoJuxtaposeCommandLineArguments()
    {
        Console.Error.WriteLine("No Juxtapose Command Line Arguments.");
        Environment.Exit((int)ExternalProcessExitCodes.NoJuxtaposeCommandLineArguments);
    }

    private static void ExitWithSuccessRunAsEndpoint()
    {
        Environment.Exit(0);
    }

    private static void WaitParentProcessExit(int parentProcessId)
    {
        Process? parentProcess = null;

        try
        {
            parentProcess = Process.GetProcessById(parentProcessId);
        }
        catch { }

        //HACK 父进程必须在运行？
        if (parentProcess is null)
        {
            Environment.Exit((int)ExternalProcessExitCodes.FindParentProcessFail);
            return;
        }

        // Exit when parent process exited
        _ = parentProcess.WaitForExitAsync(CancellationToken.None)
                         .ContinueWith(_ =>
                         {
                             Environment.Exit((int)ExternalProcessExitCodes.ParentProcessExited);
                         });
    }

    #endregion Private 方法
}