using System.Diagnostics;

namespace Juxtapose.Messages;

/// <summary>
/// 异常消息
/// </summary>
[method: DebuggerStepThrough]
public class ExceptionMessage(int ackId,
                              string originExceptionType,
                              string originMessage,
                              string? originStackTrace,
                              string originToStringValue)
    : JuxtaposeAckMessage(ackId)
{
    #region Public 属性

    /// <summary>
    /// 原始异常类型
    /// </summary>
    public string OriginExceptionType { get; [DebuggerStepThrough] set; } = originExceptionType;

    /// <summary>
    /// 异常的原始<see cref="Exception.Message"/>
    /// </summary>
    public string OriginMessage { get; [DebuggerStepThrough] set; } = originMessage;

    /// <summary>
    /// 异常的原始<see cref="Exception.StackTrace"/>
    /// </summary>
    public string? OriginStackTrace { get; [DebuggerStepThrough] set; } = originStackTrace;

    /// <summary>
    /// 原始<see cref="Exception.ToString()"/>
    /// </summary>
    public string OriginToStringValue { get; [DebuggerStepThrough] set; } = originToStringValue;

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"【ExceptionMessage】Id: {Id} ,AckId: {AckId} ,OriginMessage: {OriginMessage}";
    }

    #endregion Public 方法
}
