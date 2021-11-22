namespace Juxtapose.Messages
{
    /// <summary>
    /// 方法调用结果消息
    /// </summary>
    public abstract class MethodInvokeResultMessage<TResult>
        : JuxtaposeAckMessage
        where TResult : class
    {
        #region Public 属性

        /// <summary>
        /// 方法返回值
        /// </summary>
        public TResult? Result { get; set; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="MethodInvokeResultMessage{TResult}"/>
        public MethodInvokeResultMessage(int ackId) : base(ackId)
        {
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"【MethodInvokeResultMessage】Id: {Id} ,AckId: {AckId}";
        }

        #endregion Public 方法
    }
}