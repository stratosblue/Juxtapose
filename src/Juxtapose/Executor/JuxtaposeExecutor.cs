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
        #region Private 字段

        private IIoCContainerHolder _iocContainerHolder = EmptyIoCContainerHolder.Instance;

        private Func<ValueTask<IIoCContainerHolder>>? _iocContainerHolderGetter;

        #endregion Private 字段

        #region Protected 字段

        /// <summary>
        /// 所有对象实例
        /// </summary>
        protected internal readonly ConcurrentDictionary<int, object> ObjectInstances = new();

        #endregion Protected 字段

        #region Protected 属性

        /// <summary>
        /// 当前执行器用于创建对象的<see cref="IServiceProvider"/>
        /// </summary>
        protected IServiceProvider ServiceProvider => _iocContainerHolder.ServiceProvider;

        #endregion Protected 属性

        #region Public 属性

        /// <summary>
        /// 对象实例<inheritdoc cref="IUniqueIdGenerator"/>
        /// </summary>
        public IUniqueIdGenerator InstanceIdGenerator { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="JuxtaposeExecutor"/>
        public JuxtaposeExecutor(IMessageExchanger messageExchanger, ILogger logger, Func<ValueTask<IIoCContainerHolder>>? iocContainerHolderGetter = null) : base(messageExchanger, logger)
        {
            messageExchanger.OnInvalid += OnMessageExchangerInvalid;

            InstanceIdGenerator = new CheckedIdGenerator(ObjectInstances.ContainsKey);

            _iocContainerHolderGetter = iocContainerHolderGetter;
        }

        #endregion Public 构造函数

        #region Protected 方法

        /// <inheritdoc/>
        protected override bool Dispose(bool disposing)
        {
            if (base.Dispose(disposing))
            {
                //目前不进行同步等待
                _ = Task.Run(async () => await _iocContainerHolder.DisposeAsync());
                return true;
            }
            return false;
        }

        /// <summary>
        /// 从IoC容器获取服务实例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected TService GetRequiredService<TService>()
        {
            var rawService = ServiceProvider.GetService(typeof(TService));
            if (rawService is not TService service)
            {
                throw new InvalidOperationException($"object from service provider {rawService} can not cast to {typeof(TService)}.");
            }
            return service;
        }

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

        /// <inheritdoc/>
        public override async Task InitializationAsync(CancellationToken initializationToken)
        {
            await base.InitializationAsync(initializationToken);

            if (_iocContainerHolderGetter is not null)
            {
                _iocContainerHolder = await _iocContainerHolderGetter();

                _iocContainerHolderGetter = null;

                if (_iocContainerHolder?.ServiceProvider is null)
                {
                    throw new InvalidOperationException("there is not IoC Container can work.");
                }
            }
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