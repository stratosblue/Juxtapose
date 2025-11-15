namespace Juxtapose.Messages;

/// <summary>
/// 实例方法调用消息
/// </summary>
public class InstanceMethodInvokeMessage<TParameterPack>(int instanceId, int commandId)
    : MethodInvokeMessage<TParameterPack>(commandId)
    , IInstanceMessage
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
        return $"【InstanceMethodInvokeMessage】Id: {Id} ,CommandId: {CommandId} ,InstanceId: {InstanceId}";
    }

    #endregion Public 方法
}
