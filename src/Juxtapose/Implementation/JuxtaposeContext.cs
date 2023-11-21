﻿using Juxtapose.Communication.Channel;
using Juxtapose.Communication.Codec;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Juxtapose;

/// <summary>
/// <inheritdoc cref="IInitializationContext"/>
/// </summary>
public abstract class JuxtaposeContext : IInitializationContext, IDisposable
{
    #region Private 字段

    private IJuxtaposeBootstrapper? _bootstrapper;

    private IJuxtaposeExecutorPool? _executorPool;

    private bool _isDisposed;

    private ILoggerFactory? _loggerFactory;

    #endregion Private 字段

    #region Protected 字段

    /// <summary>
    /// 锁
    /// </summary>
    protected readonly object SyncRoot = new();

    #endregion Protected 字段

    #region Protected 属性

    /// <inheritdoc cref="IExecutorPoolPolicy"/>
    protected virtual IExecutorPoolPolicy ExecutorPoolPolicy { get; } = DefaultExecutorPoolPolicy.Instance;

    #endregion Protected 属性

    #region Public 属性

    /// <inheritdoc cref="Juxtapose.CommunicationOptions"/>
    public virtual CommunicationOptions CommunicationOptions => new(NamedPipeCommunicationChannelFactory.Shared, LengthBasedFrameCodecFactory.Shared, CreateCommunicationMessageCodecFactory());

    /// <inheritdoc/>
    public abstract string Identifier { get; }

    /// <summary>
    /// <inheritdoc cref="ILoggerFactory"/>
    /// </summary>
    public ILoggerFactory LoggerFactory { get => _loggerFactory ?? CreateLoggerFactory(); set => _loggerFactory = value; }

    /// <inheritdoc/>
    public abstract IReadOnlySettingCollection Options { get; }

    /// <inheritdoc/>
    public abstract int Version { get; }

    #endregion Public 属性

    #region Protected 方法

    /// <summary>
    /// 创建当前上下文的引导 <see cref="IJuxtaposeBootstrapper"/>
    /// </summary>
    /// <returns></returns>
    protected virtual IJuxtaposeBootstrapper CreateBootstrapper()
    {
        return new JuxtaposeBootstrapper(this, CreateExternalProcessActivator());
    }

    /// <summary>
    /// 创建当前上下文的执行器池 <see cref="IJuxtaposeExecutorPool"/>
    /// </summary>
    /// <returns></returns>
    protected virtual IJuxtaposeExecutorPool CreateExecutorPool()
    {
        return new JuxtaposeExecutorPool(this, ExecutorPoolPolicy, LoggerFactory);
    }

    /// <summary>
    /// 创建 <see cref="IExternalProcessActivator"/>
    /// </summary>
    /// <returns></returns>
    protected virtual IExternalProcessActivator? CreateExternalProcessActivator() => null;

    /// <summary>
    /// 创建 <see cref="ILoggerFactory"/>
    /// </summary>
    /// <returns></returns>
    protected virtual ILoggerFactory CreateLoggerFactory() => NullLoggerFactory.Instance;

    #region Communication

    /// <summary>
    /// 创建 <inheritdoc cref="ICommunicationMessageCodecFactory"/>
    /// </summary>
    /// <returns></returns>
    protected virtual ICommunicationMessageCodecFactory CreateCommunicationMessageCodecFactory()
    {
        var communicationMessageCodec = new DefaultJsonBasedMessageCodec(GetMessageTypes(), LoggerFactory, null);
        return new GenericSharedMessageCodecFactory(communicationMessageCodec);
    }

    /// <summary>
    /// 获取所有消息类型
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<Type> GetMessageTypes();

    #endregion Communication

    #endregion Protected 方法

    #region Public 方法

    #region IDisposable

    /// <summary>
    ///
    /// </summary>
    ~JuxtaposeContext()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc cref="Dispose()"/>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _bootstrapper?.Dispose();
            _executorPool?.Dispose();
            _loggerFactory = null;
            _bootstrapper = null;
            _executorPool = null;
        }
    }

    /// <summary>
    ///
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(JuxtaposeContext));
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    /// <inheritdoc/>
    public abstract JuxtaposeExecutor CreateExecutor(IMessageExchanger messageExchanger);

    /// <inheritdoc/>
    public virtual IJuxtaposeBootstrapper GetBootstrapper()
    {
        ThrowIfDisposed();
        if (_bootstrapper is not null)
        {
            return _bootstrapper;
        }
        lock (SyncRoot)
        {
            if (_bootstrapper is not null)
            {
                return _bootstrapper;
            }
            return _bootstrapper = CreateBootstrapper();
        }
    }

    /// <inheritdoc/>
    public virtual IJuxtaposeExecutorPool GetExecutorPool()
    {
        ThrowIfDisposed();
        if (_executorPool is not null)
        {
            return _executorPool;
        }
        lock (SyncRoot)
        {
            if (_executorPool is not null)
            {
                return _executorPool;
            }
            return _executorPool = CreateExecutorPool();
        }
    }

    #endregion Public 方法
}
