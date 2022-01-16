using System;

namespace Juxtapose.Messages.ParameterPacks
{
    /// <summary>
    /// 从 <see cref="IServiceProvider"/> 获取对象实例消息
    /// </summary>
    public sealed class ServiceProviderGetInstanceParameterPack
    {
        #region Public 属性

        /// <summary>
        /// 类型完整名称
        /// </summary>
        public string TypeFullName { get; set; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="ServiceProviderGetInstanceParameterPack"/>
        public ServiceProviderGetInstanceParameterPack(string typeFullName)
        {
            if (string.IsNullOrWhiteSpace(typeFullName))
            {
                throw new ArgumentException($"“{nameof(typeFullName)}”不能为 null 或空白。", nameof(typeFullName));
            }

            TypeFullName = typeFullName;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Create {TypeFullName} with ServiceProvider";
        }

        #endregion Public 方法
    }
}