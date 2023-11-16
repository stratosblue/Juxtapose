using System.Runtime.Serialization;

namespace Juxtapose;

/// <summary>
/// 通道相关异常
/// </summary>
[Serializable]
public class ChannelException : JuxtaposeException
{
    #region Public 构造函数

    /// <inheritdoc cref="ChannelException"/>
    public ChannelException(string? message) : base(message)
    {
    }

    #endregion Public 构造函数

    #region Protected 构造函数

    /// <inheritdoc cref="ChannelException"/>
    protected ChannelException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    #endregion Protected 构造函数
}
