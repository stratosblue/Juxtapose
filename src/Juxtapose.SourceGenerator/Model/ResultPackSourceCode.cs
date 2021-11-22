﻿using System;
using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model
{
    /// <summary>
    /// 返回值包源代码
    /// </summary>
    public class ResultPackSourceCode : ArgumentPackSourceCode
    {
        #region Public 属性

        /// <summary>
        /// 当前源代码对应的方法符号
        /// </summary>
        public IMethodSymbol MethodSymbol { get; }

        #endregion Public 属性

        #region Public 构造函数

        public ResultPackSourceCode(IMethodSymbol methodSymbol, string hintName, string source, string @namespace, string typeName, string typeFullName)
            : base(hintName, source, @namespace, typeName, typeFullName)
        {
            MethodSymbol = methodSymbol ?? throw new ArgumentNullException(nameof(methodSymbol));
        }

        #endregion Public 构造函数
    }
}