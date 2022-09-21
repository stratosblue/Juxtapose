using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator;

internal static class BuildEnvironment
{
    #region Private 字段

    private static readonly Dictionary<string, INamedTypeSymbol> s_storedNamedTypeSymbol = new();

    private static bool s_isInited = false;

    #endregion Private 字段

    #region Public 属性

    public static INamedTypeSymbol CancellationToken { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public static INamedTypeSymbol DelegateSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public static INamedTypeSymbol TaskSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public static INamedTypeSymbol TaskTSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public static INamedTypeSymbol ValueTaskSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public static INamedTypeSymbol ValueTaskTSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }
    public static INamedTypeSymbol VoidSymbol { get => GetNamedTypeSymbol(); private set => SetNamedTypeSymbol(value); }

    #endregion Public 属性

    #region Private 方法

    private static INamedTypeSymbol GetNamedTypeSymbol([CallerMemberName] string propName = null!)
    {
        if (s_storedNamedTypeSymbol.TryGetValue(propName, out var namedTypeSymbol))
        {
            return namedTypeSymbol;
        }

        throw new InvalidOperationException($"{propName} not init yet");
    }

    private static void SetNamedTypeSymbol(INamedTypeSymbol namedTypeSymbol, [CallerMemberName] string propName = null!)
    {
        s_storedNamedTypeSymbol[propName] = namedTypeSymbol;
    }

    #endregion Private 方法

    #region Public 方法

    public static void Init(Compilation compilation)
    {
        if (s_isInited)
        {
            return;
        }

        //TODO 缓存好像有问题

        TaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task") ?? throw new InvalidOperationException();
        TaskTSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1") ?? throw new InvalidOperationException();
        ValueTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask") ?? throw new InvalidOperationException();
        ValueTaskTSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1") ?? throw new InvalidOperationException();
        VoidSymbol = compilation.GetSpecialType(SpecialType.System_Void) ?? throw new InvalidOperationException();
        CancellationToken = compilation.GetTypeByMetadataName("System.Threading.CancellationToken") ?? throw new InvalidOperationException();
        DelegateSymbol = compilation.GetSpecialType(SpecialType.System_Delegate) ?? throw new InvalidOperationException();

        s_isInited = true;
    }

    #endregion Public 方法
}