namespace Juxtapose;

/// <summary>
/// 默认 <inheritdoc cref="IExecutorPoolPolicy"/>
/// </summary>
public class DefaultExecutorPoolPolicy : IExecutorPoolPolicy
{
    #region Singleton

    private static readonly WeakReference<DefaultExecutorPoolPolicy?> s_instance = new(null);

    /// <summary>
    /// 实例
    /// </summary>
    public static DefaultExecutorPoolPolicy Instance => GetInstance();

    private static DefaultExecutorPoolPolicy GetInstance()
    {
        if (s_instance.TryGetTarget(out var policy))
        {
            return policy;
        }
        lock (s_instance)
        {
            if (s_instance.TryGetTarget(out policy))
            {
                return policy;
            }
            policy = new();
            s_instance.SetTarget(policy);
            return policy;
        }
    }

    #endregion Singleton

    #region Public 方法

    /// <inheritdoc/>
    public string Classify(ExecutorCreationContext creationContext, out int? holdLimit)
    {
        holdLimit = null;
        if (creationContext.IsStatic)
        {
            return $"{creationContext.TargetType}:{creationContext.TargetType.TypeHandle.Value}";
        }
        return $"{creationContext.TargetType}:{creationContext.TargetIdentifier}:{Guid.NewGuid():N}";
    }

    /// <inheritdoc/>
    public bool ShouldDropExecutor(ExecutorCreationContext creationContext, IJuxtaposeExecutorHolder executorHolder)
    {
        return !creationContext.IsStatic;
    }

    #endregion Public 方法
}
