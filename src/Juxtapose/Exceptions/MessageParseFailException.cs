using System;
using System.Runtime.Serialization;

namespace Juxtapose;

/// <summary>
/// 消息解析异常
/// </summary>
[Serializable]
public class MessageParseFailException : JuxtaposeException
{
    #region Public 构造函数

    /// <inheritdoc cref="MessageParseFailException"/>
    public MessageParseFailException()
    {
    }

    /// <inheritdoc cref="MessageParseFailException"/>
    public MessageParseFailException(string? message) : base(message)
    {
    }

    #endregion Public 构造函数

    #region Protected 构造函数

    /// <inheritdoc cref="MessageParseFailException"/>
    protected MessageParseFailException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    #endregion Protected 构造函数
}