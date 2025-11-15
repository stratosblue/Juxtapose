namespace Juxtapose.Messages;

/// <summary>
/// 创建对象实例消息
/// </summary>
public class CreateObjectInstanceMessage<TParameterPack>(int instanceId, int commandId)
    : MethodInvokeMessage<TParameterPack>(commandId)
{
    #region Public 属性

    /// <summary>
    /// 要创建的实例ID
    /// </summary>
    public int InstanceId { get; set; } = instanceId;

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【CreateObjectInstanceMessage】Id: {Id} ,CommandId: {CommandId} ,InstanceId: {InstanceId} ,ParameterPack: {ParameterPack}";
    }

    #endregion Public 方法
}
