namespace Juxtapose.Messages;

/// <summary>
/// Juxtapose 消息
/// </summary>
public abstract class JuxtaposeMessage
{
    #region Private 字段

    private int _id;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 消息ID
    /// </summary>
    public int Id { get => _id; set => _id = value > 0 ? value : throw new JuxtaposeException($"{nameof(Id)} must be greater than 0."); }

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【JuxtaposeMessage】Id: {Id}";
    }

    #endregion Public 方法
}
