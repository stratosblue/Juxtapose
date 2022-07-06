using Juxtapose.SourceGenerator;

namespace Microsoft.CodeAnalysis;

internal static class ITypeSymbolExtensions
{
    #region Public 方法

    public static bool IsAwaitable(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.IsTask()
               || typeSymbol.IsValueTask()
               || typeSymbol.IsValueTaskT()
               || typeSymbol.IsTaskT()
               || (typeSymbol.BaseType is not null
                   && typeSymbol.BaseType.IsAwaitable());
    }

    public static bool IsCancellationToken(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.Equals(BuildEnvironment.CancellationToken, SymbolEqualityComparer.Default);
    }

    public static bool IsDelegate(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.TypeKind == TypeKind.Delegate;
        //return typeSymbol.Equals(BuildEnvironment.DelegateSymbol, SymbolEqualityComparer.Default)
        //       || (typeSymbol.BaseType is not null
        //           && typeSymbol.BaseType.IsDelegate());
    }

    public static bool IsTask(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.Equals(BuildEnvironment.TaskSymbol, SymbolEqualityComparer.Default);
    }

    public static bool IsTaskT(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.OriginalDefinition.Equals(BuildEnvironment.TaskTSymbol, SymbolEqualityComparer.Default);
    }

    public static bool IsValueTask(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.Equals(BuildEnvironment.ValueTaskSymbol, SymbolEqualityComparer.Default);
    }

    public static bool IsValueTaskT(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.OriginalDefinition.Equals(BuildEnvironment.ValueTaskTSymbol, SymbolEqualityComparer.Default);
    }

    public static bool IsVoid(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.Equals(BuildEnvironment.VoidSymbol, SymbolEqualityComparer.Default);
    }

    #endregion Public 方法
}