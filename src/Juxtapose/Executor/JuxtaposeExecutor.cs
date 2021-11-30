using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Juxtapose.Messages;
using Juxtapose.Messages.ParameterPacks;

using Microsoft.Extensions.Logging;

namespace Juxtapose
{
    /// <summary>
    /// Juxtapose 执行器
    /// </summary>
    public abstract class JuxtaposeExecutor : MessageDispatcher
    {
        #region Protected 字段

        /// <summary>
        /// 所有对象实例
        /// </summary>
        protected internal readonly ConcurrentDictionary<int, object> ObjectInstances = new();

        #endregion Protected 字段

        #region Public 属性

        /// <summary>
        /// 对象实例<inheritdoc cref="IUniqueIdGenerator"/>
        /// </summary>
        public IUniqueIdGenerator InstanceIdGenerator { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="JuxtaposeExecutor"/>
        public JuxtaposeExecutor(IMessageExchanger messageExchanger, ILogger logger) : base(messageExchanger, logger)
        {
            messageExchanger.OnInvalid += OnMessageExchangerInvalid;

            InstanceIdGenerator = new CheckedIdGenerator(ObjectInstances.ContainsKey);
        }

        #endregion Public 构造函数

        #region Protected 方法

        /// <summary>
        /// 在消息交换器失效时的回调
        /// </summary>
        /// <param name="obj"></param>
        protected virtual void OnMessageExchangerInvalid(object obj)
        {
            Dispose();
        }

        #endregion Protected 方法

        #region Public 方法

        /// <summary>
        /// 添加一个对象的实例
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public virtual void AddObjectInstance(int instanceId, object instance)
        {
            Logger.LogTrace("Try add instance [{0}] for id [{1}]", instance, instanceId);
            if (!ObjectInstances.TryAdd(instanceId, instance ?? throw new ArgumentNullException(nameof(instance))))
            {
                throw new InstanceDuplicateException(instanceId);
            }
        }

        /// <summary>
        /// 释放远程对象实例
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public virtual void DisposeObjectInstance(int instanceId)
        {
            Task.Run(async () =>
            {
                Logger.LogTrace("Dispose object instance. Id [{0}]", instanceId);
                try
                {
                    await InvokeMessageAsync(new DisposeObjectInstanceMessage(instanceId), RunningToken);
                }
                catch (Exception ex)
                {
                    if (!IsDisposed)
                    {
                        Logger.LogWarning(ex, "Dispose object instance Fail. Id [{0}]", instanceId);
                    }
                }
            }, RunningToken);
        }

        /// <summary>
        /// 获取一个对象的实例
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public object GetObjectInstance(int instanceId)
        {
            Logger.LogTrace("Get instance by id [{0}]", instanceId);
            if (ObjectInstances.TryGetValue(instanceId, out var instance))
            {
                return instance;
            }
            ThrowIfDisposed();
            throw new InstanceNotFoundException(instanceId);
        }

        /// <summary>
        /// 移除一个对象的实例
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public void RemoveObjectInstance(int instanceId)
        {
            Logger.LogTrace("Try remove instance by id [{0}]", instanceId);
            ObjectInstances.TryRemove(instanceId, out _);
        }

        /// <summary>
        /// 尝试获取关联的<see cref="IExternalProcess"/>
        /// </summary>
        /// <param name="externalProcess"></param>
        /// <returns></returns>
        public bool TryGetExternalProcess([NotNullWhen(true)] out IExternalProcess? externalProcess)
        {
            if (MessageExchanger is IExternalWorker externalWorker)
            {
                externalProcess = externalWorker.ExternalProcess;
                return true;
            }
            externalProcess = null;
            return false;
        }

        #endregion Public 方法

        #region Protected 方法

        /// <inheritdoc/>
        protected override Task<JuxtaposeMessage?> OnMessageAsync(JuxtaposeMessage message, CancellationToken cancellation)
        {
            if (message is DisposeObjectInstanceMessage disposeObjectInstanceMessage)
            {
                if (ObjectInstances.TryRemove(disposeObjectInstanceMessage.InstanceId, out var storedObject))
                {
                    Logger.LogTrace("Dispose object instance. Id [{0}]", disposeObjectInstanceMessage.InstanceId);
                    if (storedObject is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                else
                {
                    Logger.LogWarning("Dispose object instance message received. But not found it in executor. Id [{0}]", disposeObjectInstanceMessage.InstanceId);
                }
                return Task.FromResult<JuxtaposeMessage?>(null);
            }

            //处理非Ack的实例调用消息
            if (message is IInstanceMessage instanceMessage
                && message is not JuxtaposeAckMessage)
            {
                if (message is InstanceMethodInvokeMessage<CancellationTokenSourceCancelParameterPack> cancelMessage)
                {
                    //TODO 当取消消息在执行消息之前到达时，将无法正确取消
                    if (ObjectInstances.TryRemove(cancelMessage.InstanceId, out var storedObject))
                    {
                        Logger.LogTrace("CancellationTokenSource cancel message received. Id [{0}]", cancelMessage.InstanceId);
                        ((CancellationTokenSource)storedObject).Cancel();
                        return Task.FromResult<JuxtaposeMessage?>(null);
                    }
                    else
                    {
                        Logger.LogWarning("CancellationTokenSource cancel message received. But not found it in executor. Id [{0}]", cancelMessage.InstanceId);
                        //如果在此处添加对象，可以保证取消，但需要一个恰当的锁定和移除机制
                        return Task.FromResult<JuxtaposeMessage?>(null);
                    }
                }
                else
                {
                    return ((IMessageExecutor)GetObjectInstance(instanceMessage.InstanceId)).ExecuteAsync(this, message);
                }
            }
            return base.OnMessageAsync(message, cancellation);
        }

        #endregion Protected 方法
    }
}