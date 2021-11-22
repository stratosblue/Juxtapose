using System;
using System.Collections.Generic;
using System.Linq;

using Juxtapose.SourceGenerator;

namespace Microsoft.CodeAnalysis
{
    internal static class INamedTypeSymbolExtensions
    {
        #region Public 方法

        public static IEnumerable<INamedTypeSymbol> AllInterfaces(this INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.TypeKind != TypeKind.Interface)
            {
                throw new ArgumentException($"{namedTypeSymbol} not a interface.");
            }

            yield return namedTypeSymbol;
            foreach (var item in namedTypeSymbol.AllInterfaces)
            {
                yield return item;
            }
        }

        public static IEnumerable<ISymbol> GetProxyableMembers(this INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.TypeKind == TypeKind.Interface)
            {
                return namedTypeSymbol.AllInterfaces().SelectMany(GetProxyableMembersDirectly);
            }
            return GetProxyableMembersDirectly(namedTypeSymbol);

            static bool NotStaticConstructorMethod(ISymbol symbol)
            {
                return symbol is IMethodSymbol methodSymbol
                       && methodSymbol.MethodKind != MethodKind.StaticConstructor
                       && methodSymbol.MethodKind != MethodKind.SharedConstructor;
            }

            static IEnumerable<ISymbol> GetProxyableMembersDirectly(INamedTypeSymbol namedTypeSymbol)
            {
                return namedTypeSymbol.GetMembers().Where(m => m is IPropertySymbol || NotStaticConstructorMethod(m));
            }
        }

        public static bool IsBaseOnJuxtaposeContext(this INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.BaseType?.ToDisplayString() == TypeFullNames.Juxtapose.JuxtaposeContext_NoGlobal;
        }

        #endregion Public 方法
    }
}