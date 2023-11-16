using System.Runtime.Serialization;

namespace Juxtapose;

/// <summary>
/// 未初始化异常
/// </summary>
[Serializable]
public class NotInitializedException : JuxtaposeException
{
    #region Public 属性

    /// <summary>
    ///
    /// </summary>
    public string TargetName { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="NotInitializedException"/>
    public NotInitializedException(string targetName) : base($"{targetName} Not Initialized.")
    {
        TargetName = targetName;
    }

    #endregion Public 构造函数

    #region Protected 构造函数

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    /// <inheritdoc cref="NotInitializedException"/>
    protected NotInitializedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    #endregion Protected 构造函数
}
