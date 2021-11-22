namespace Juxtapose
{
    /// <summary>
    /// 实例未找到异常
    /// </summary>
    public class InstanceNotFoundException : JuxtaposeException
    {
        #region Public 属性

        /// <summary>
        /// 实例ID
        /// </summary>
        public int InstanceId { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="InstanceNotFoundException"/>
        public InstanceNotFoundException(int instanceId) : base($"{instanceId} Not Found.")
        {
            InstanceId = instanceId;
        }

        #endregion Public 构造函数
    }
}