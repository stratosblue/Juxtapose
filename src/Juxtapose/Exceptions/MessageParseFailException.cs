namespace Juxtapose
{
    /// <summary>
    /// 消息解析异常
    /// </summary>
    public class MessageParseFailException : JuxtaposeException
    {
        #region Public 构造函数

        /// <inheritdoc cref="MessageParseFailException"/>
        public MessageParseFailException()
        {
        }

        /// <inheritdoc cref="MessageParseFailException"/>
        public MessageParseFailException(string? message) : base(message)
        {
        }

        #endregion Public 构造函数
    }
}