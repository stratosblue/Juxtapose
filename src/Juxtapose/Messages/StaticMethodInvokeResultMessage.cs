namespace Juxtapose.Messages;

/// <summary>
/// 静态方法调用结果消息
/// </summary>
public class StaticMethodInvokeResultMessage<TResult>
    : MethodInvokeResultMessage<TResult>
{
    #region Public 构造函数

    /// <inheritdoc cref="StaticMethodInvokeResultMessage{TResult}"/>
    public StaticMethodInvokeResultMessage(int ackId) : base(ackId)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【StaticMethodInvokeResultMessage】Id: {Id} ,AckId: {AckId}";
    }

    #endregion Public 方法
}
