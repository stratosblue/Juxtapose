namespace Juxtapose.Messages;

/// <summary>
/// 实例方法调用结果消息
/// </summary>
public class InstanceMethodInvokeResultMessage<TResult>
    : MethodInvokeResultMessage<TResult>
    , IInstanceMessage
{
    #region Public 属性

    /// <summary>
    /// 实例ID
    /// </summary>
    public int InstanceId { get; set; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="InstanceMethodInvokeResultMessage{TResult}"/>
    public InstanceMethodInvokeResultMessage(int ackId, int instanceId) : base(ackId)
    {
        InstanceId = instanceId;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【InstanceMethodInvokeResultMessage】Id: {Id} ,AckId: {AckId} ,InstanceId: {InstanceId}";
    }

    #endregion Public 方法
}
