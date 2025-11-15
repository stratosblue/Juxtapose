using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Juxtapose;

/// <inheritdoc cref="IMessageExchangerFactory"/>
public sealed class MessageExchangerFactory(ICommunicationChannelFactory communicationChannelFactory,
                                            ICommunicationFrameCodecFactory communicationFrameCodecFactory,
                                            ICommunicationMessageCodecFactory communicationMessageCodecFactory,
                                            ILoggerFactory loggerFactory)
    : IMessageExchangerFactory
{
    #region Private 字段

    private readonly ICommunicationChannelFactory _communicationChannelFactory = communicationChannelFactory ?? throw new ArgumentNullException(nameof(communicationChannelFactory));

    private readonly ICommunicationFrameCodecFactory _communicationFrameCodecFactory = communicationFrameCodecFactory ?? throw new ArgumentNullException(nameof(communicationFrameCodecFactory));

    private readonly ICommunicationMessageCodecFactory _communicationMessageCodecFactory = communicationMessageCodecFactory ?? throw new ArgumentNullException(nameof(communicationMessageCodecFactory));

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// <see cref="ILoggerFactory"/>
    /// </summary>
    public ILoggerFactory LoggerFactory { get; } = loggerFactory ?? NullLoggerFactory.Instance;

    #endregion Public 属性

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
