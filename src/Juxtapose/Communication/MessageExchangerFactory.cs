using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Juxtapose;

/// <inheritdoc cref="IMessageExchangerFactory"/>
public sealed class MessageExchangerFactory : IMessageExchangerFactory
{
    #region Private 字段

    private readonly ICommunicationChannelFactory _communicationChannelFactory;

    private readonly ICommunicationFrameCodecFactory _communicationFrameCodecFactory;

    private readonly ICommunicationMessageCodecFactory _communicationMessageCodecFactory;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// <see cref="ILoggerFactory"/>
    /// </summary>
    public ILoggerFactory LoggerFactory { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="MessageExchangerFactory"/>
    public MessageExchangerFactory(ICommunicationChannelFactory communicationChannelFactory,
                                   ICommunicationFrameCodecFactory communicationFrameCodecFactory,
                                   ICommunicationMessageCodecFactory communicationMessageCodecFactory,
                                   ILoggerFactory loggerFactory)
    {
        _communicationChannelFactory = communicationChannelFactory ?? throw new ArgumentNullException(nameof(communicationChannelFactory));
        _communicationFrameCodecFactory = communicationFrameCodecFactory ?? throw new ArgumentNullException(nameof(communicationFrameCodecFactory));
        _communicationMessageCodecFactory = communicationMessageCodecFactory ?? throw new ArgumentNullException(nameof(communicationMessageCodecFactory));
        LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public Task<IMessageExchanger> CreateAsync(IJuxtaposeOptions options, CancellationToken cancellation = default)
    {
        var messageExchanger = new MessageExchanger(_communicationChannelFactory.CreateClient(options),
                                                    _communicationFrameCodecFactory.Create(options),
                                                    _communicationMessageCodecFactory.Create(options),
                                                    LoggerFactory);
        return Task.FromResult<IMessageExchanger>(messageExchanger);
    }

    /// <inheritdoc/>
    public Task<IMessageExchanger> CreateHostAsync(IJuxtaposeOptions options, CancellationToken cancellation = default)
    {
        var messageExchanger = new MessageExchanger(_communicationChannelFactory.CreateServer(options),
                                                    _communicationFrameCodecFactory.Create(options),
                                                    _communicationMessageCodecFactory.Create(options),
                                                    LoggerFactory);
        return Task.FromResult<IMessageExchanger>(messageExchanger);
    }

    /// <inheritdoc/>
    public void Dispose()
    { }

    #endregion Public 方法
}
