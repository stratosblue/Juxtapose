namespace Juxtapose.Messages;

/// <summary>
/// 静态方法调用结果消息
/// </summary>
public class StaticMethodInvokeResultMessage<TResult>(int ackId)
    : MethodInvokeResultMessage<TResult>(ackId)
{
    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【StaticMethodInvokeResultMessage】Id: {Id} ,AckId: {AckId}";
    }

    #endregion Public 方法
}
