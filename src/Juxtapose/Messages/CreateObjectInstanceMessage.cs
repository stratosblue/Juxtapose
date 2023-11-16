namespace Juxtapose.Messages;

/// <summary>
/// 创建对象实例消息
/// </summary>
public class CreateObjectInstanceMessage<TParameterPack>
    : MethodInvokeMessage<TParameterPack>
{
    #region Public 属性

    /// <summary>
    /// 要创建的实例ID
    /// </summary>
    public int InstanceId { get; set; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="CreateObjectInstanceMessage{TParameterPack}"/>
    public CreateObjectInstanceMessage(int instanceId, int commandId) : base(commandId)
    {
        InstanceId = instanceId;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【CreateObjectInstanceMessage】Id: {Id} ,CommandId: {CommandId} ,InstanceId: {InstanceId} ,ParameterPack: {ParameterPack}";
    }

    #endregion Public 方法
}
