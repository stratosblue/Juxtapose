using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Juxtapose.Communication.Codec;

/// <summary>
///
/// </summary>
public class LengthBasedFrameCodec : ICommunicationFrameCodec
{
    #region Public 方法

    /// <inheritdoc/>
    public bool TryGetMessageFrame(ReadOnlySequence<byte> buffer, [NotNullWhen(true)] out ReadOnlySequence<byte>? frameBuffer)
    {
        //0长度帧逻辑上不应该存在，但是还是防御性的 >=
        if (buffer.Length >= sizeof(long))
        {
            Span<byte> lengthBuffer = stackalloc byte[sizeof(long)];
            buffer.Slice(0, sizeof(long)).CopyTo(lengthBuffer);
            var length = BitConverter.ToInt64(lengthBuffer);

            if (length <= buffer.Length - sizeof(long))
            {
                frameBuffer = buffer.Slice(sizeof(long), length);
                return true;
            }
        }

        frameBuffer = null;
        return false;
    }

    /// <inheritdoc/>
    public ValueTask WriteMessageFrameAsync(IBufferWriter<byte> bufferWriter, ReadOnlyMemory<byte> data)
    {
        bufferWriter.Write(BitConverter.GetBytes((long)data.Length));
        bufferWriter.Write(data.Span);
        return ValueTask.CompletedTask;
    }

    #endregion Public 方法
}
