using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Juxtapose.Communication.Codec
{
    /// <summary>
    ///
    /// </summary>
    public class LengthBasedFrameCodec : ICommunicationFrameCodec
    {
        #region Public 方法

        /// <inheritdoc/>
        public bool TryGetMessageFrame(ReadOnlySequence<byte> buffer, [NotNullWhen(true)] out ReadOnlySequence<byte>? frameBuffer)
        {
            if (buffer.Length > sizeof(long))
            {
                var length = BitConverter.ToInt64(buffer.Slice(0, sizeof(long)).ToArray());
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
}