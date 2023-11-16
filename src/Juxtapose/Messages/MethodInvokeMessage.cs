namespace Juxtapose.Messages;

/// <summary>
/// 方法调用消息
/// </summary>
/// <typeparam name="TParameterPack">方法参数包类型</typeparam>
public abstract class MethodInvokeMessage<TParameterPack>
    : JuxtaposeCommandMessage
{
    #region Public 属性

    /// <summary>
    /// 方法参数包
    /// </summary>
    public TParameterPack? ParameterPack { get; set; }

    #endregion Public 属性

    #region Protected 构造函数

    /// <summary>
    /// <inheritdoc cref="MethodInvokeMessage{TParameterPack}"/>
    /// </summary>
    /// <param name="commandId">指令ID</param>
    protected MethodInvokeMessage(int commandId) : base(commandId)
    { }

    #endregion Protected 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【{nameof(MethodInvokeMessage<TParameterPack>)}】Id: {Id}";
    }

    #endregion Public 方法
}
