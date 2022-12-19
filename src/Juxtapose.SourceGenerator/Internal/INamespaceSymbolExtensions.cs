namespace Microsoft.CodeAnalysis;

internal static class INamespaceSymbolExtensions
{
    #region Public 方法

    public static string? GetNamespaceName(this INamespaceSymbol? namespaceSymbol)
    {
        return namespaceSymbol is null || namespaceSymbol.IsGlobalNamespace
               ? null
               : namespaceSymbol.ToDisplayString();
    }

    #endregion Public 方法
}