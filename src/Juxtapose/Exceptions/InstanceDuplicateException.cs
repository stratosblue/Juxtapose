namespace Juxtapose
{
    /// <summary>
    /// 实例重复异常
    /// </summary>
    public class InstanceDuplicateException : JuxtaposeException
    {
        #region Public 属性

        /// <summary>
        /// 实例ID
        /// </summary>
        public int InstanceId { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="InstanceDuplicateException"/>
        public InstanceDuplicateException(int instanceId) : base($" Duplicate id - {instanceId} for object.")
        {
            InstanceId = instanceId;
        }

        #endregion Public 构造函数
    }
}