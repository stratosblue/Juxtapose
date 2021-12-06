using System;
using System.Runtime.Serialization;

namespace Juxtapose
{
    /// <summary>
    /// 实例重复异常
    /// </summary>
    [Serializable]
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

        #region Protected 构造函数

        /// <inheritdoc cref="InstanceDuplicateException"/>
        protected InstanceDuplicateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion Protected 构造函数
    }
}