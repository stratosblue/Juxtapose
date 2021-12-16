using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model
{
    /// <summary>
    /// 参数包源代码
    /// </summary>
    public class ParameterPackSourceCode : ArgumentPackSourceCode
    {
        #region Public 属性

        public static IEqualityComparer<ParameterPackSourceCode> EqualityComparer { get; } = new DefaultEqualityComparer();

        #endregion Public 属性

        #region Public 构造函数

        public ParameterPackSourceCode(IMethodSymbol methodSymbol, string hintName, string source, string @namespace, string typeName, string typeFullName)
            : base(methodSymbol, hintName, source, @namespace, typeName, typeFullName)
        {
        }

        #endregion Public 构造函数

        #region Private 类

        private class DefaultEqualityComparer : IEqualityComparer<ParameterPackSourceCode>
        {
            #region Public 方法

            public bool Equals(ParameterPackSourceCode x, ParameterPackSourceCode y)
            {
                if (x is ConstructorParameterPackSourceCode
                    || y is ConstructorParameterPackSourceCode)
                {
                    var cx = x as ConstructorParameterPackSourceCode;
                    var cy = y as ConstructorParameterPackSourceCode;
                    return cx is not null
                           && cy is not null
                           && SymbolEqualityComparer.Default.Equals(cx.MethodSymbol, cy.MethodSymbol)
                           && string.Equals(cx.GeneratedTypeName, cy.GeneratedTypeName, StringComparison.Ordinal);
                }
                return SymbolEqualityComparer.Default.Equals(x.MethodSymbol, y.MethodSymbol);
            }

            public int GetHashCode(ParameterPackSourceCode obj)
            {
                if (obj is ConstructorParameterPackSourceCode constructorSourceCode)
                {
                    return SymbolEqualityComparer.Default.GetHashCode(constructorSourceCode.MethodSymbol)
                           & constructorSourceCode.GetHashCode();
                }
                return SymbolEqualityComparer.Default.GetHashCode(obj.MethodSymbol);
            }

            #endregion Public 方法
        }

        #endregion Private 类
    }
}