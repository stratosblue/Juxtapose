using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis;

public class TypeSymbolAnalyzer
{
    #region Private 字段

    private readonly ConcurrentDictionary<string, INamedTypeSymbol> _storedNamedTypeSymbol = new();

    #endregion Private 字段

    #region Public 属性

    public INamedTypeSymbol CancellationToken { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public INamedTypeSymbol DelegateSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public INamedTypeSymbol TaskSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public INamedTypeSymbol TaskTSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public INamedTypeSymbol ValueTaskSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public INamedTypeSymbol ValueTaskTSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public INamedTypeSymbol VoidSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }

    #endregion Public 属性

    #region Public 构造函数

    public TypeSymbolAnalyzer(Compilation compilation)
    {
        TaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task") ?? throw new InvalidOperationException();
        TaskTSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1") ?? throw new InvalidOperationException();
        ValueTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask") ?? throw new InvalidOperationException();
        ValueTaskTSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1") ?? throw new InvalidOperationException();
        VoidSymbol = compilation.GetSpecialType(SpecialType.System_Void) ?? throw new InvalidOperationException();
        CancellationToken = compilation.GetTypeByMetadataName("System.Threading.CancellationToken") ?? throw new InvalidOperationException();
        DelegateSymbol = compilation.GetSpecialType(SpecialType.System_Delegate) ?? throw new InvalidOperationException();
    }

    #endregion Public 构造函数

    #region Private 方法

    private INamedTypeSymbol GetNamedTypeSymbol([CallerMemberName] string propName = null!)
        => _storedNamedTypeSymbol.TryGetValue(propName, out var namedTypeSymbol)
           ? namedTypeSymbol
           : throw new InvalidOperationException($"{propName} not init yet");

    private void SetNamedTypeSymbol(INamedTypeSymbol namedTypeSymbol, [CallerMemberName] string propName = null!) => _storedNamedTypeSymbol[propName] = namedTypeSymbol;

    #endregion Private 方法

    #region Public 方法

    /// <summary>
    /// 获取方法的返回类型（当返回类型为Task`T`时，返回T的类型）当类型为 void 时，返回null
    /// </summary>
    /// <param name="methodSymbol"></param>
    /// <returns></returns>
    public ITypeSymbol? GetReturnType(IMethodSymbol methodSymbol)
    {
        var returnType = methodSymbol.ReturnType;
        if (returnType is INamedTypeSymbol namedTypeSymbol
            && (IsTaskT(namedTypeSymbol) || IsValueTaskT(namedTypeSymbol)))
        {
            return namedTypeSymbol.TypeArguments[0];
        }
        return IsVoid(returnType) || IsTask(returnType) || IsValueTask(returnType) ? null : returnType;
    }

    public bool IsReturnVoidOrTask(IMethodSymbol methodSymbol)
    {
        return methodSymbol.ReturnsVoid
               || IsTask(methodSymbol.ReturnType)
               || IsValueTask(methodSymbol.ReturnType)
               ;
    }

    public bool IsAwaitable(ITypeSymbol typeSymbol)
    {
        return IsTask(typeSymbol)
               || IsValueTask(typeSymbol)
               || IsValueTaskT(typeSymbol)
               || IsTaskT(typeSymbol)
               || (typeSymbol.BaseType is not null
                   && IsAwaitable(typeSymbol.BaseType));
    }

    public bool IsCancellationToken(ITypeSymbol typeSymbol)
    {
        return typeSymbol.Equals(CancellationToken, SymbolEqualityComparer.Default);
    }

    public bool IsDelegate(ITypeSymbol typeSymbol)
    {
        return typeSymbol.TypeKind == TypeKind.Delegate;
    }

    public bool IsTask(ITypeSymbol typeSymbol)
    {
        return typeSymbol.Equals(TaskSymbol, SymbolEqualityComparer.Default);
    }

    public bool IsTaskT(ITypeSymbol typeSymbol)
    {
        return typeSymbol.OriginalDefinition.Equals(TaskTSymbol, SymbolEqualityComparer.Default);
    }

    public bool IsValueTask(ITypeSymbol typeSymbol)
    {
        return typeSymbol.Equals(ValueTaskSymbol, SymbolEqualityComparer.Default);
    }

    public bool IsValueTaskT(ITypeSymbol typeSymbol)
    {
        return typeSymbol.OriginalDefinition.Equals(ValueTaskTSymbol, SymbolEqualityComparer.Default);
    }

    public bool IsVoid(ITypeSymbol typeSymbol)
    {
        return typeSymbol.Equals(VoidSymbol, SymbolEqualityComparer.Default);
    }

    #endregion Public 方法
}