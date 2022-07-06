namespace Juxtapose;

/// <summary>
/// 帧编码器工厂
/// </summary>
public interface ICommunicationFrameCodecFactory
{
    #region Public 方法

    /// <summary>
    /// 创建 <see cref="ICommunicationFrameCodec"/>
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    ICommunicationFrameCodec Create(IJuxtaposeOptions options);

    #endregion Public 方法
}