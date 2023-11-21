﻿namespace Juxtapose.Messages;

/// <summary>
/// 实例方法调用消息
/// </summary>
public class InstanceMethodInvokeMessage<TParameterPack>
    : MethodInvokeMessage<TParameterPack>
    , IInstanceMessage
{
    #region Public 属性

    /// <summary>
    /// 实例ID
    /// </summary>
    public int InstanceId { get; set; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="InstanceMethodInvokeMessage{TParameterPack}"/>
    public InstanceMethodInvokeMessage(int instanceId, int commandId) : base(commandId)
    {
        InstanceId = instanceId;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【InstanceMethodInvokeMessage】Id: {Id} ,CommandId: {CommandId} ,InstanceId: {InstanceId}";
    }

    #endregion Public 方法
}
