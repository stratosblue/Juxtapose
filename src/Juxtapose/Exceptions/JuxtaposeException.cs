using System.Runtime.Serialization;

namespace Juxtapose;

/// <summary>
/// Juxtapose异常
/// </summary>
[Serializable]
public class JuxtaposeException : Exception
{
    #region Public 构造函数

    /// <inheritdoc cref="JuxtaposeException"/>
    public JuxtaposeException()
    {
    }

    /// <inheritdoc cref="JuxtaposeException"/>
    public JuxtaposeException(string? message) : base(message)
    {
    }

    /// <inheritdoc cref="JuxtaposeException"/>
    public JuxtaposeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    #endregion Public 构造函数

    #region Protected 构造函数

    /// <inheritdoc cref="JuxtaposeException"/>
#pragma warning disable SYSLIB0051 // 类型或成员已过时
    protected JuxtaposeException(SerializationInfo info, StreamingContext context) : base(info, context)
#pragma warning restore SYSLIB0051 // 类型或成员已过时
    {
    }

    #endregion Protected 构造函数
}
