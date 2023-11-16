namespace Juxtapose;

/// <summary>
/// 通用共享消息编解码器
/// </summary>
public class GenericSharedMessageCodecFactory : ICommunicationMessageCodecFactory
{
    #region Private 字段

    private readonly ICommunicationMessageCodec _messageCodec;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="GenericSharedMessageCodecFactory"/>
    public GenericSharedMessageCodecFactory(ICommunicationMessageCodec communicationMessageCodec)
    {
        _messageCodec = communicationMessageCodec ?? throw new ArgumentNullException(nameof(communicationMessageCodec));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public ICommunicationMessageCodec Create(IJuxtaposeOptions options) => _messageCodec;

    #endregion Public 方法
}
