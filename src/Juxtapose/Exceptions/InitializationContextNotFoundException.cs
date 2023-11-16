using System.Runtime.Serialization;

namespace Juxtapose;

/// <summary>
/// 上下文未找到异常
/// </summary>
[Serializable]
public class InitializationContextNotFoundException : JuxtaposeException
{
    #region Public 构造函数

    /// <inheritdoc cref="InitializationContextNotFoundException"/>
    public InitializationContextNotFoundException(string identifier) : base($"context - {identifier} not found.")
    {
    }

    #endregion Public 构造函数

    #region Protected 构造函数

    /// <inheritdoc cref="InitializationContextNotFoundException"/>
    protected InitializationContextNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    #endregion Protected 构造函数
}
