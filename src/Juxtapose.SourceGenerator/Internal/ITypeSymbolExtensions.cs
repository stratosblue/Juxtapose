namespace Microsoft.CodeAnalysis;

internal static class ITypeSymbolExtensions
{
    #region Public 方法

    public static bool IsDelegate(this ITypeSymbol typeSymbol) => typeSymbol.TypeKind == TypeKind.Delegate;

    #endregion Public 方法
}
