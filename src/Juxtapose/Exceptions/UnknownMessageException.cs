namespace Juxtapose
{
    /// <summary>
    /// 未知异常
    /// </summary>
    public class UnknownMessageException : JuxtaposeException
    {
        #region Public 属性

        /// <summary>
        ///
        /// </summary>
        public object UnknownMessage { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="UnknownMessageException"/>
        public UnknownMessageException(object unknownMessage) : base($"UnknownMessage - {unknownMessage}")
        {
            UnknownMessage = unknownMessage;
        }

        /// <inheritdoc cref="UnknownMessageException"/>
        public UnknownMessageException(object unknownMessage, string? message) : base(message)
        {
            UnknownMessage = unknownMessage;
        }

        #endregion Public 构造函数
    }
}