namespace Juxtapose;

/// <summary>
/// 传输选项
/// </summary>
public class CommunicationOptions
{
    #region Private 字段

    private ICommunicationChannelFactory _channelFactory;

    private ICommunicationFrameCodecFactory _frameCodecFactory;

    private ICommunicationMessageCodecFactory _messageCodecFactory;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// <inheritdoc cref="ICommunicationChannelFactory"/>
    /// </summary>
    public virtual ICommunicationChannelFactory ChannelFactory { get => _channelFactory; set => _channelFactory = value ?? throw new ArgumentNullException(nameof(value)); }

    /// <summary>
    /// <inheritdoc cref="ICommunicationFrameCodecFactory"/>
    /// </summary>
    public virtual ICommunicationFrameCodecFactory FrameCodecFactory { get => _frameCodecFactory; set => _frameCodecFactory = value ?? throw new ArgumentNullException(nameof(value)); }

    /// <summary>
    /// <inheritdoc cref="ICommunicationMessageCodecFactory"/>
    /// </summary>
    public virtual ICommunicationMessageCodecFactory MessageCodecFactory { get => _messageCodecFactory; set => _messageCodecFactory = value ?? throw new ArgumentNullException(nameof(value)); }

    /// <summary>
    /// <inheritdoc cref="IMessageExchangerFactory"/>
    /// </summary>
    public virtual IMessageExchangerFactory? MessageExchangerFactory { get; set; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="CommunicationOptions"/>
    public CommunicationOptions(ICommunicationChannelFactory channelFactory, ICommunicationFrameCodecFactory frameCodecFactory, ICommunicationMessageCodecFactory messageCodecFactory)
    {
        _channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
        _frameCodecFactory = frameCodecFactory ?? throw new ArgumentNullException(nameof(frameCodecFactory));
        _messageCodecFactory = messageCodecFactory ?? throw new ArgumentNullException(nameof(messageCodecFactory));
    }

    #endregion Public 构造函数
}
