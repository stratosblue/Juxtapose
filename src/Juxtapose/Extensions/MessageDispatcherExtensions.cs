using System;
using System.Threading;
using System.Threading.Tasks;

using Juxtapose.Messages;

namespace Juxtapose
{
    /// <summary>
    ///
    /// </summary>
    public static class MessageDispatcherExtensions
    {
        #region Public 方法

        #region Async

        /// <summary>
        /// 发送执行实例方法的消息
        /// </summary>
        /// <typeparam name="TParameterPack"></typeparam>
        /// <param name="dispatcher"></param>
        /// <param name="parameterPack"></param>
        /// <param name="instanceId"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static Task<JuxtaposeAckMessage> InvokeInstanceMethodMessageAsync<TParameterPack>(this MessageDispatcher dispatcher,
                                                                                                 TParameterPack parameterPack,
                                                                                                 int instanceId,
                                                                                                 CancellationToken cancellation = default)
            where TParameterPack : class
        {
            var message = new InstanceMethodInvokeMessage<TParameterPack>(instanceId) { ParameterPack = parameterPack };
            return dispatcher.InvokeMessageAsync(message, cancellation);
        }

        /// <summary>
        /// 发送执行实例方法的消息
        /// </summary>
        /// <typeparam name="TParameterPack"></typeparam>
        /// <typeparam name="TResultPack"></typeparam>
        /// <param name="dispatcher"></param>
        /// <param name="parameterPack"></param>
        /// <param name="instanceId"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static async Task<TResultPack> InvokeInstanceMethodMessageAsync<TParameterPack, TResultPack>(this MessageDispatcher dispatcher,
                                                                                                            TParameterPack parameterPack,
                                                                                                            int instanceId,
                                                                                                            CancellationToken cancellation = default)
            where TParameterPack : class
            where TResultPack : class
        {
            var message = new InstanceMethodInvokeMessage<TParameterPack>(instanceId) { ParameterPack = parameterPack };
            var ackMessage = await dispatcher.InvokeMessageAsync(message, cancellation);

            return ((InstanceMethodInvokeResultMessage<TResultPack>)ackMessage).Result!;
        }

        /// <summary>
        /// 发送执行实例方法的消息
        /// </summary>
        /// <typeparam name="TParameterPack"></typeparam>
        /// <param name="dispatcher"></param>
        /// <param name="parameterPack"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static Task<JuxtaposeAckMessage> InvokeStaticMethodMessageAsync<TParameterPack>(this MessageDispatcher dispatcher,
                                                                                               TParameterPack parameterPack,
                                                                                               CancellationToken cancellation = default)
            where TParameterPack : class
        {
            var message = new StaticMethodInvokeMessage<TParameterPack>() { ParameterPack = parameterPack };
            return dispatcher.InvokeMessageAsync(message, cancellation);
        }

        /// <summary>
        /// 发送执行实例方法的消息
        /// </summary>
        /// <typeparam name="TParameterPack"></typeparam>
        /// <typeparam name="TResultPack"></typeparam>
        /// <param name="dispatcher"></param>
        /// <param name="parameterPack"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static async Task<TResultPack> InvokeStaticMethodMessageAsync<TParameterPack, TResultPack>(this MessageDispatcher dispatcher,
                                                                                                          TParameterPack parameterPack,
                                                                                                          CancellationToken cancellation = default)
            where TParameterPack : class
            where TResultPack : class
        {
            var message = new StaticMethodInvokeMessage<TParameterPack>() { ParameterPack = parameterPack };
            var ackMessage = await dispatcher.InvokeMessageAsync(message, cancellation);

            return ((StaticMethodInvokeResultMessage<TResultPack>)ackMessage).Result!;
        }

        #endregion Async

        #region Sync

        private static async Task<TResult> InvokeWithoutSyncContextAsync<TResult>(Func<Task<TResult>> action)
        {
            await SynchronizationContextRemover.Awaiter;
            return await action();
        }

        /// <summary>
        /// 发送执行实例方法的消息
        /// </summary>
        /// <typeparam name="TParameterPack"></typeparam>
        /// <param name="dispatcher"></param>
        /// <param name="parameterPack"></param>
        /// <param name="instanceId"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static JuxtaposeAckMessage InvokeInstanceMethodMessage<TParameterPack>(this MessageDispatcher dispatcher,
                                                                                      TParameterPack parameterPack,
                                                                                      int instanceId,
                                                                                      CancellationToken cancellation = default)
            where TParameterPack : class
        {
            return InvokeWithoutSyncContextAsync(() => dispatcher.InvokeInstanceMethodMessageAsync(parameterPack, instanceId, cancellation)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 发送执行实例方法的消息
        /// </summary>
        /// <typeparam name="TParameterPack"></typeparam>
        /// <typeparam name="TResultPack"></typeparam>
        /// <param name="dispatcher"></param>
        /// <param name="parameterPack"></param>
        /// <param name="instanceId"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>

        public static TResultPack InvokeInstanceMethodMessage<TParameterPack, TResultPack>(this MessageDispatcher dispatcher,
                                                                                           TParameterPack parameterPack,
                                                                                           int instanceId,
                                                                                           CancellationToken cancellation = default)
            where TParameterPack : class
            where TResultPack : class
        {
            return InvokeWithoutSyncContextAsync(() => dispatcher.InvokeInstanceMethodMessageAsync<TParameterPack, TResultPack>(parameterPack, instanceId, cancellation)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 发送执行实例方法的消息
        /// </summary>
        /// <typeparam name="TParameterPack"></typeparam>
        /// <param name="dispatcher"></param>
        /// <param name="parameterPack"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static JuxtaposeAckMessage InvokeStaticMethodMessage<TParameterPack>(this MessageDispatcher dispatcher,
                                                                                    TParameterPack parameterPack,
                                                                                    CancellationToken cancellation = default)
            where TParameterPack : class
        {
            return InvokeWithoutSyncContextAsync(() => dispatcher.InvokeStaticMethodMessageAsync<TParameterPack>(parameterPack, cancellation)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 发送执行实例方法的消息
        /// </summary>
        /// <typeparam name="TParameterPack"></typeparam>
        /// <typeparam name="TResultPack"></typeparam>
        /// <param name="dispatcher"></param>
        /// <param name="parameterPack"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public static TResultPack InvokeStaticMethodMessage<TParameterPack, TResultPack>(this MessageDispatcher dispatcher,
                                                                                         TParameterPack parameterPack,
                                                                                         CancellationToken cancellation = default)
            where TParameterPack : class
            where TResultPack : class
        {
            return InvokeWithoutSyncContextAsync(() => dispatcher.InvokeStaticMethodMessageAsync<TParameterPack, TResultPack>(parameterPack, cancellation)).GetAwaiter().GetResult();
        }

        #endregion Sync

        #endregion Public 方法
    }
}