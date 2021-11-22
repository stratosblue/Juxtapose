using System;

namespace Juxtapose.SourceGenerator
{
    /// <summary>
    /// 幻象静态类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class IllusionStaticClassAttribute : Attribute
    {
        #region Public 属性

        /// <summary>
        /// 可访问性
        /// </summary>
        public IllusionClassAccessibility Accessibility { get; }

        /// <summary>
        /// 代理类型名
        /// </summary>
        public string? ProxyTypeName { get; }

        /// <summary>
        /// 静态类类型
        /// </summary>
        public Type Type { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="IllusionClassAttribute"/>
        /// </summary>
        /// <param name="type">静态类类型</param>
        /// <param name="proxyTypeName">指定生成的代理类型名称</param>
        /// <param name="accessibility">可访问性</param>
        public IllusionStaticClassAttribute(Type type, string? proxyTypeName = null, IllusionClassAccessibility accessibility = IllusionClassAccessibility.Default)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            if (!(type.IsAbstract && type.IsSealed))
            {
                throw new ArgumentException($"{type} must be a static class type.");
            }
            ProxyTypeName = proxyTypeName;
            Accessibility = accessibility;
        }

        #endregion Public 构造函数
    }
}