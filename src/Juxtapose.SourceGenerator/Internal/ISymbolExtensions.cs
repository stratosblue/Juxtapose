namespace Microsoft.CodeAnalysis;

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

    #region ToFullyQualifiedDisplayString

    /// <summary>
    /// modified from <see cref="SymbolDisplayFormat.FullyQualifiedFormat"/>
    /// </summary>
    private static readonly SymbolDisplayFormat s_fullyQualifiedFormat = new(SymbolDisplayGlobalNamespaceStyle.Included,
                                                                             SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                                                                             SymbolDisplayGenericsOptions.IncludeTypeParameters,
                                                                             SymbolDisplayMemberOptions.None,
                                                                             SymbolDisplayDelegateStyle.NameOnly,
                                                                             SymbolDisplayExtensionMethodStyle.Default,
                                                                             SymbolDisplayParameterOptions.None,
                                                                             SymbolDisplayPropertyStyle.NameOnly,
                                                                             SymbolDisplayLocalOptions.None,
                                                                             SymbolDisplayKindOptions.None,
                                                                             SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static string ToFullyQualifiedDisplayString(this ISymbol symbol)
    {
        return symbol.ToDisplayString(s_fullyQualifiedFormat);
    }

    public static string ToInheritDocCrefString(this ISymbol symbol)
    {
        return symbol.ToDisplayString().Replace('<', '{').Replace('>', '}');
    }

    #endregion ToFullyQualifiedDisplayString

    #endregion Public 方法
}
