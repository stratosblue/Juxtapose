using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Juxtapose
{
    /// <summary>
    /// 帧编码器
    /// </summary>
    public interface ICommunicationFrameCodec
    {
        #region Public 方法

        /// <summary>
        /// 尝试获取数据帧
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="frameBuffer">一个完整的数据帧buffer</param>
        /// <returns></returns>
        bool TryGetMessageFrame(ReadOnlySequence<byte> buffer, [NotNullWhen(true)] out ReadOnlySequence<byte>? frameBuffer);

        /// <summary>
        /// 写入数据帧
        /// </summary>
        /// <param name="bufferWriter"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        ValueTask WriteMessageFrameAsync(IBufferWriter<byte> bufferWriter, ReadOnlyMemory<byte> data);

        #endregion Public 方法
    }
}