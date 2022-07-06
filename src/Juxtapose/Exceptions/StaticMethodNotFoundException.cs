using System;
using System.Runtime.Serialization;

namespace Juxtapose;

/// <summary>
/// 静态方法未找到异常
/// </summary>
[Serializable]
public class StaticMethodNotFoundException : JuxtaposeException
{
    #region Public 属性

    /// <summary>
    /// 消息ID
    /// </summary>
    public int MessageId { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="InstanceNotFoundException"/>
    public StaticMethodNotFoundException(int messageId) : base($"Static Method Not Found for message - {messageId}.")
    {
        MessageId = messageId;
    }

    #endregion Public 构造函数

    #region Protected 构造函数

    /// <inheritdoc cref="StaticMethodNotFoundException"/>
    protected StaticMethodNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    #endregion Protected 构造函数
}