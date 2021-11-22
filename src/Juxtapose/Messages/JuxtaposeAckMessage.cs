using System.Diagnostics;

namespace Juxtapose.Messages
{
    /// <summary>
    /// Juxtapose 确认消息
    /// </summary>
    public class JuxtaposeAckMessage : JuxtaposeMessage
    {
        #region Public 属性

        /// <summary>
        /// 确认的消息ID
        /// </summary>
        public int AckId { [DebuggerStepThrough]get; [DebuggerStepThrough]set; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="JuxtaposeAckMessage"/>
        [DebuggerStepThrough]
        public JuxtaposeAckMessage(int ackId)
        {
            AckId = ackId;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"【JuxtaposeAckMessage】Id: {Id} ,AckId: {AckId}";
        }

        #endregion Public 方法
    }
}