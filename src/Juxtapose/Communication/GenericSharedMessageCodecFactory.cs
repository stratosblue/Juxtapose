namespace Juxtapose;

/// <summary>
/// 通用共享消息编解码器
/// </summary>
public class GenericSharedMessageCodecFactory(ICommunicationMessageCodec communicationMessageCodec)
    : ICommunicationMessageCodecFactory
{
    #region Private 字段

    private readonly ICommunicationMessageCodec _messageCodec = communicationMessageCodec ?? throw new ArgumentNullException(nameof(communicationMessageCodec));

    #endregion Private 字段

    #region Public 方法

    /// <inheritdoc/>
    public ICommunicationMessageCodec Create(IJuxtaposeOptions options) => _messageCodec;

    #endregion Public 方法
}
