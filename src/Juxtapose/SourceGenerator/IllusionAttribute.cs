﻿using System;

#pragma warning disable IDE0060 // 删除未使用的参数

namespace Juxtapose.SourceGenerator
{
    /// <summary>
    /// 制造幻象类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class IllusionAttribute : Attribute
    {
        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="IllusionAttribute"/>
        /// </summary>
        /// <param name="targetType">目标类型</param>
        /// <param name="inheritType">生成时需要继承的类型（当前只能是接口）</param>
        /// <param name="generatedTypeName">生成的类型名称</param>
        /// <param name="accessibility">可访问性</param>
        /// <param name="fromIoCContainer">需要对象实例时，从IoC容器中创建对象</param>
        public IllusionAttribute(Type targetType,   //TODO 允许为接口
                                 Type? inheritType = null,  //TODO 支持继承类
                                 string? generatedTypeName = null,
                                 GeneratedAccessibility accessibility = GeneratedAccessibility.Default,
                                 bool fromIoCContainer = false)
        {
        }

        #endregion Public 构造函数
    }
}