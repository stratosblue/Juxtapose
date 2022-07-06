using System;
using System.Collections.Generic;
using System.Linq;

using Juxtapose.SourceGenerator;

namespace Microsoft.CodeAnalysis;

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

    public static IEnumerable<ISymbol> GetProxyableMembers(this INamedTypeSymbol namedTypeSymbol, bool withConstructor)
    {
        if (namedTypeSymbol.TypeKind == TypeKind.Interface)
        {
            return namedTypeSymbol.AllInterfaces().SelectMany(GetProxyableMembersDirectly);
        }
        return GetProxyableMembersDirectly(namedTypeSymbol);

        bool IsProxyableConstructorMethod(ISymbol symbol)
        {
            //TODO Internal？
            return symbol is IMethodSymbol methodSymbol
                   && methodSymbol.DeclaredAccessibility == Accessibility.Public
                   && methodSymbol.MethodKind != MethodKind.StaticConstructor
                   && methodSymbol.MethodKind != MethodKind.SharedConstructor
                   && (withConstructor || methodSymbol.MethodKind != MethodKind.Constructor);
        }

        IEnumerable<ISymbol> GetProxyableMembersDirectly(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.GetMembers().Where(m => m is IPropertySymbol || IsProxyableConstructorMethod(m));
        }
    }

    public static bool IsBaseOnJuxtaposeContext(this INamedTypeSymbol namedTypeSymbol)
    {
        if (namedTypeSymbol.BaseType is null)
        {
            return false;
        }
        return namedTypeSymbol.BaseType.ToDisplayString() == TypeFullNames.Juxtapose.JuxtaposeContext_NoGlobal
               || (namedTypeSymbol.BaseType.BaseType is not null
                   && namedTypeSymbol.BaseType.BaseType.IsBaseOnJuxtaposeContext());
    }

    /// <summary>
    /// 是否从类型<paramref name="baseType"/>派生
    /// </summary>
    /// <param name="namedTypeSymbol"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    public static bool IsDerivedFrom(this INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol baseType)
    {
        return namedTypeSymbol.BaseType is not null
               && (SymbolEqualityComparer.Default.Equals(namedTypeSymbol.BaseType, baseType)
                   || namedTypeSymbol.BaseType.IsDerivedFrom(baseType));
    }

    #endregion Public 方法
}