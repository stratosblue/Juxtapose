using Juxtapose.Messages;

namespace Juxtapose;

/// <summary>
///
/// </summary>
public static class IInitializationContextExtensions
{
    #region Public 方法

    /// <summary>
    /// 从 <paramref name="context"/> 中获取 <see cref="IJuxtaposeExecutorOwner"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="creationContext"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static IJuxtaposeExecutorOwner GetExecutorOwner(this IInitializationContext context,
                                                           ExecutorCreationContext creationContext,
                                                           CancellationToken cancellation = default)
    {
        return GetExecutorOwnerWithoutSyncContextAsync(context, creationContext, cancellation).GetAwaiter().GetResult();

        static async Task<IJuxtaposeExecutorOwner> GetExecutorOwnerWithoutSyncContextAsync(IInitializationContext context,
                                                                                           ExecutorCreationContext creationContext,
                                                                                           CancellationToken cancellation)
        {
            await SynchronizationContextRemover.Awaiter;
            return await context.GetExecutorOwnerAsync(creationContext, cancellation);
        }
    }

    /// <summary>
    /// 从 <paramref name="context"/> 中获取 <see cref="IJuxtaposeExecutorOwner"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="creationContext"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static async Task<IJuxtaposeExecutorOwner> GetExecutorOwnerAsync(this IInitializationContext context,
                                                                            ExecutorCreationContext creationContext,
                                                                            CancellationToken cancellation)
    {
        var executorPool = context.GetExecutorPool();
        var executorOwner = await executorPool.GetAsync(creationContext, cancellation);
        return executorOwner;
    }

    /// <summary>
    /// 获取<paramref name="context"/>中的<see cref="IMessageExchangerFactory"/>或者创建一个新的
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static IMessageExchangerFactory GetOrCreateMessageExchangerFactory(this IInitializationContext context)
    {
        if (context?.CommunicationOptions is not CommunicationOptions options)
        {
            throw new ArgumentNullException(nameof(context), $"{nameof(IInitializationContext.CommunicationOptions)} is null.");
        }

        return options.MessageExchangerFactory
               ?? new MessageExchangerFactory(options.ChannelFactory,
                                              options.FrameCodecFactory,
                                              options.MessageCodecFactory,
                                              context.LoggerFactory);
    }

    #endregion Public 方法

    #region Message

    /// <summary>
    /// 发送执行静态方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <typeparam name="TResultPack"></typeparam>
    /// <param name="context"></param>
    /// <param name="creationContext"></param>
    /// <param name="parameterPack"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static TResultPack InvokeStaticMessage<TParameterPack, TResultPack>(this IInitializationContext context,
                                                                               ExecutorCreationContext creationContext,
                                                                               TParameterPack parameterPack,
                                                                               int commandId,
                                                                               CancellationToken cancellation)
    {
        return InvokeStaticMessageWithoutSyncContextAsync(context, creationContext, parameterPack, commandId, cancellation).GetAwaiter().GetResult();

        static async Task<TResultPack> InvokeStaticMessageWithoutSyncContextAsync(IInitializationContext context,
                                                                                  ExecutorCreationContext creationContext,
                                                                                  TParameterPack parameterPack,
                                                                                  int commandId,
                                                                                  CancellationToken cancellation)
        {
            await SynchronizationContextRemover.Awaiter;
            return await context.InvokeStaticMessageAsync<TParameterPack, TResultPack>(creationContext, parameterPack, commandId, cancellation);
        }
    }

    /// <summary>
    /// 发送执行静态方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <param name="context"></param>
    /// <param name="creationContext"></param>
    /// <param name="parameterPack"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static JuxtaposeAckMessage InvokeStaticMessage<TParameterPack>(this IInitializationContext context,
                                                                          ExecutorCreationContext creationContext,
                                                                          TParameterPack parameterPack,
                                                                          int commandId,
                                                                          CancellationToken cancellation)
    {
        return InvokeStaticMessageWithoutSyncContextAsync(context, creationContext, parameterPack, commandId, cancellation).GetAwaiter().GetResult();

        static async Task<JuxtaposeAckMessage> InvokeStaticMessageWithoutSyncContextAsync(IInitializationContext context,
                                                                                          ExecutorCreationContext creationContext,
                                                                                          TParameterPack parameterPack,
                                                                                          int commandId,
                                                                                          CancellationToken cancellation)
        {
            await SynchronizationContextRemover.Awaiter;
            return await context.InvokeStaticMessageAsync<TParameterPack>(creationContext, parameterPack, commandId, cancellation);
        }
    }

    /// <summary>
    /// 发送执行静态方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <typeparam name="TResultPack"></typeparam>
    /// <param name="context"></param>
    /// <param name="creationContext"></param>
    /// <param name="parameterPack"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static async Task<TResultPack> InvokeStaticMessageAsync<TParameterPack, TResultPack>(this IInitializationContext context,
                                                                                                ExecutorCreationContext creationContext,
                                                                                                TParameterPack parameterPack,
                                                                                                int commandId,
                                                                                                CancellationToken cancellation)
    {
        using var executorOwner = await context.GetExecutorOwnerAsync(creationContext, cancellation);
        return await executorOwner.Executor.InvokeStaticMethodMessageAsync<TParameterPack, TResultPack>(parameterPack, commandId, cancellation);
    }

    /// <summary>
    /// 发送执行静态方法的消息
    /// </summary>
    /// <typeparam name="TParameterPack"></typeparam>
    /// <param name="context"></param>
    /// <param name="creationContext"></param>
    /// <param name="parameterPack"></param>
    /// <param name="commandId"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    public static async Task<JuxtaposeAckMessage> InvokeStaticMessageAsync<TParameterPack>(this IInitializationContext context,
                                                                                           ExecutorCreationContext creationContext,
                                                                                           TParameterPack parameterPack,
                                                                                           int commandId,
                                                                                           CancellationToken cancellation)
    {
        using var executorOwner = await context.GetExecutorOwnerAsync(creationContext, cancellation);
        return await executorOwner.Executor.InvokeStaticMethodMessageAsync<TParameterPack>(parameterPack, commandId, cancellation);
    }

    #endregion Message
}
