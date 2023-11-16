using System.Buffers;

namespace Juxtapose;

/// <summary>
/// 消息编解码器
/// </summary>
public interface ICommunicationMessageCodec
{
    #region Public 方法

    /// <summary>
    /// 解码数据为对象
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    object Decode(ReadOnlySequence<byte> buffer);

    /// <summary>
    /// 编码对象为数据
    /// </summary>
    /// <param name="message"></param>
    /// <param name="bufferWriter"></param>
    /// <returns></returns>
    ValueTask<long> Encode(object message, IBufferWriter<byte> bufferWriter);

    #endregion Public 方法
}
