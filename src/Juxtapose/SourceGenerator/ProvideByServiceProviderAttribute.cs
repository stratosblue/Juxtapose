using System;

#pragma warning disable IDE0060 // 删除未使用的参数

namespace Juxtapose.SourceGenerator
{
    /// <summary>
    /// 指定哪些类型通过 DI (<see cref="IServiceProvider"/>) 提供，不生成具体构造方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ProvideByServiceProviderAttribute : Attribute
    {
        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="ProvideByServiceProviderAttribute"/>
        /// </summary>
        /// <param name="types">由DI提供的类型列表</param>
        public ProvideByServiceProviderAttribute(params Type[] types)
        {
        }

        #endregion Public 构造函数
    }
}