namespace Juxtapose;

/// <summary>
/// 消息编解码器工厂
/// </summary>
public interface ICommunicationMessageCodecFactory
{
    #region Public 方法

    /// <summary>
    /// 创建 <see cref="ICommunicationMessageCodec"/>
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    ICommunicationMessageCodec Create(IJuxtaposeOptions options);

    #endregion Public 方法
}
