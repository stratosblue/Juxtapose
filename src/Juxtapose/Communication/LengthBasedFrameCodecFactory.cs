using System;
using System.Threading;

using Juxtapose.Communication.Codec;

namespace Juxtapose
{
    /// <summary>
    ///
    /// </summary>
    public class LengthBasedFrameCodecFactory : ICommunicationFrameCodecFactory
    {
        #region static shared

        private static readonly Lazy<LengthBasedFrameCodecFactory> s_shared = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// 共享的 <see cref="LengthBasedFrameCodecFactory"/> 实例
        /// </summary>
        public static LengthBasedFrameCodecFactory Shared => s_shared.Value;

        #endregion static shared

        #region Private 字段

        private readonly LengthBasedFrameCodec _lengthBasedFrameCodec = new();

        #endregion Private 字段

        #region Public 方法

        /// <inheritdoc/>
        public ICommunicationFrameCodec Create(IJuxtaposeOptions options) => _lengthBasedFrameCodec;

        #endregion Public 方法
    }
}