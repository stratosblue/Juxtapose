namespace Juxtapose
{
    /// <summary>
    /// 通道相关异常
    /// </summary>
    public class ChannelException : JuxtaposeException
    {
        #region Public 构造函数

        /// <inheritdoc cref="ChannelException"/>
        public ChannelException(string? message) : base(message)
        {
        }

        #endregion Public 构造函数
    }
}