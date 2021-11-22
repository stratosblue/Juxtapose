using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis
{
    internal static class ISymbolExtensions
    {
        #region Public 方法

        public static string GetAccessibilityCodeString(this ISymbol symbol)
        {
            return symbol.DeclaredAccessibility.ToCodeString();
        }

        public static string GetIdentifier(this ISymbol symbol)
        {
            return symbol.ToDisplayString().CalculateMd5();
        }

        public static IEnumerable<IMethodSymbol> GetMethodSymbols(this IEnumerable<ISymbol> symbols)
        {
            foreach (var item in symbols)
            {
                switch (item)
                {
                    case IPropertySymbol propertySymbol:
                        if (propertySymbol.SetMethod is not null)
                        {
                            yield return propertySymbol.SetMethod;
                        }
                        if (propertySymbol.GetMethod is not null)
                        {
                            yield return propertySymbol.GetMethod;
                        }
                        break;

                    case IMethodSymbol methodSymbol:
                        yield return methodSymbol;
                        break;

                    default:
                        //TODO 支持其他的属性
                        continue;
                };
            }
        }

        #endregion Public 方法
    }
}