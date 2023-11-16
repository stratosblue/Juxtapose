namespace Juxtapose;

/// <summary>
/// 执行器创建上下文
/// </summary>
public class ExecutorCreationContext
{
    #region Public 字段

    /// <summary>
    /// 是否是主机端
    /// </summary>
    public readonly bool IsHosting;

    /// <summary>
    /// 是否为静态调用
    /// </summary>
    public readonly bool IsStatic;

    /// <summary>
    /// 选项
    /// </summary>
    public readonly IJuxtaposeOptions? Options;

    /// <summary>
    /// 目标标识
    /// </summary>
    public readonly string TargetIdentifier;

    /// <summary>
    /// 使用执行器的目标类型
    /// </summary>
    public readonly Type TargetType;

    #endregion Public 字段

    #region Public 构造函数

    /// <inheritdoc cref="ExecutorCreationContext"/>
    public ExecutorCreationContext(Type targetType, string targetIdentifier, bool isStatic, bool isHosting, IJuxtaposeOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(targetIdentifier))
        {
            throw new ArgumentException($"“{nameof(targetIdentifier)}”不能为 null 或空白。", nameof(targetIdentifier));
        }

        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        TargetIdentifier = targetIdentifier;
        IsStatic = isStatic;
        IsHosting = isHosting;
        Options = options;
    }

    #endregion Public 构造函数
}
