using System.Diagnostics;

namespace Juxtapose.Messages;

/// <summary>
/// 释放对象实例消息
/// </summary>
[method: DebuggerStepThrough]
public class DisposeObjectInstanceMessage(int instanceId) : JuxtaposeMessage
{
    #region Public 属性

    /// <summary>
    /// 实例ID
    /// </summary>
    public int InstanceId { get; set; } = instanceId;

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【DisposeObjectInstanceMessage】Id: {Id} ,InstanceId: {InstanceId}";
    }

    #endregion Public 方法
}
