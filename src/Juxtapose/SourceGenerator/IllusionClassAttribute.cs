using System;

namespace Juxtapose.SourceGenerator
{
    /// <summary>
    /// 幻象类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class IllusionClassAttribute : Attribute
    {
        #region Public 属性

        /// <summary>
        /// 可访问性
        /// </summary>
        public IllusionClassAccessibility Accessibility { get; }

        /// <summary>
        /// 实现类型
        /// </summary>
        public Type ImplementType { get; }

        /// <summary>
        /// 接口类型
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// 代理类型名
        /// </summary>
        public string? ProxyTypeName { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="IllusionClassAttribute"/>
        /// </summary>
        /// <param name="interfaceType">接口类型</param>
        /// <param name="implementType">实现类型</param>
        /// <param name="proxyTypeName">指定生成的代理类型名称</param>
        /// <param name="accessibility">可访问性</param>
        public IllusionClassAttribute(Type interfaceType, Type implementType, string? proxyTypeName = null, IllusionClassAccessibility accessibility = IllusionClassAccessibility.Default)
        {
            InterfaceType = interfaceType ?? throw new ArgumentNullException(nameof(interfaceType));
            ImplementType = implementType ?? throw new ArgumentNullException(nameof(implementType));
            ProxyTypeName = proxyTypeName;
            Accessibility = accessibility;
        }

        #endregion Public 构造函数
    }
}