using System;
using System.Runtime.Serialization;

namespace Juxtapose;

/// <summary>
/// 未知异常
/// </summary>
[Serializable]
public class UnknownMessageException : JuxtaposeException
{
    #region Public 属性

    /// <summary>
    ///
    /// </summary>
    public object UnknownMessage { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="UnknownMessageException"/>
    public UnknownMessageException(object unknownMessage) : base($"UnknownMessage - {unknownMessage}")
    {
        UnknownMessage = unknownMessage;
    }

    /// <inheritdoc cref="UnknownMessageException"/>
    public UnknownMessageException(object unknownMessage, string? message) : base(message)
    {
        UnknownMessage = unknownMessage;
    }

    #endregion Public 构造函数

    #region Protected 构造函数

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    /// <inheritdoc cref="UnknownMessageException"/>
    protected UnknownMessageException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    #endregion Protected 构造函数
}