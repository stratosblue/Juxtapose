namespace Juxtapose.Messages;

/// <summary>
/// 实例方法调用结果消息
/// </summary>
public class InstanceMethodInvokeResultMessage<TResult>(int ackId, int instanceId)
    : MethodInvokeResultMessage<TResult>(ackId)
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
        return $"【InstanceMethodInvokeResultMessage】Id: {Id} ,AckId: {AckId} ,InstanceId: {InstanceId}";
    }

    #endregion Public 方法
}
