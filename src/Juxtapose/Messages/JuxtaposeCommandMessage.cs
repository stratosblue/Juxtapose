namespace Juxtapose.Messages;

/// <summary>
/// 指令消息
/// </summary>
public abstract class JuxtaposeCommandMessage
    : JuxtaposeMessage
{
    #region Public 属性

    /// <summary>
    /// 指令ID
    /// </summary>
    public int CommandId { get; }

    #endregion Public 属性

    #region Protected 构造函数

    /// <summary>
    /// <inheritdoc cref="JuxtaposeCommandMessage"/>
    /// </summary>
    /// <param name="commandId">指令ID</param>
    protected JuxtaposeCommandMessage(int commandId)
    {
        CommandId = commandId;
    }

    #endregion Protected 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{base.ToString()} ,CommandId: {CommandId}";
    }

    #endregion Public 方法
}
