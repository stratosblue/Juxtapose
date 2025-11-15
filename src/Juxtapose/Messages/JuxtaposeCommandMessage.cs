namespace Juxtapose.Messages;

/// <summary>
/// 指令消息
/// </summary>
/// <param name="commandId">指令ID</param>
public abstract class JuxtaposeCommandMessage(int commandId)
    : JuxtaposeMessage
{
    #region Public 属性

    /// <summary>
    /// 指令ID
    /// </summary>
    public int CommandId { get; } = commandId;

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{base.ToString()} ,CommandId: {CommandId}";
    }

    #endregion Public 方法
}
