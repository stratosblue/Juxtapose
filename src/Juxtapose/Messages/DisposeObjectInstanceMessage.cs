using System.Diagnostics;

namespace Juxtapose.Messages;

/// <summary>
/// 释放对象实例消息
/// </summary>
public class DisposeObjectInstanceMessage : JuxtaposeMessage
{
    #region Public 属性

    /// <summary>
    /// 实例ID
    /// </summary>
    public int InstanceId { get; set; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="DisposeObjectInstanceMessage"/>
    [DebuggerStepThrough]
    public DisposeObjectInstanceMessage(int instanceId)
    {
        InstanceId = instanceId;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【DisposeObjectInstanceMessage】Id: {Id} ,InstanceId: {InstanceId}";
    }

    #endregion Public 方法
}
