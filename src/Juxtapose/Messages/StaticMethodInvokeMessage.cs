namespace Juxtapose.Messages;

/// <summary>
/// 静态方法调用消息
/// </summary>
public class StaticMethodInvokeMessage<TParameterPack>
    : MethodInvokeMessage<TParameterPack>
{
    #region Public 构造函数

    /// <inheritdoc cref="StaticMethodInvokeMessage{TParameterPack}"/>
    public StaticMethodInvokeMessage(int commandId) : base(commandId)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【StaticMethodInvokeMessage】Id: {Id}";
    }

    #endregion Public 方法
}
