using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator
{
    internal static class BuildEnvironment
    {
        #region Private 字段

        private static readonly Dictionary<string, INamedTypeSymbol> s_storedNamedTypeSymbol = new();

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

        public static void Init(GeneratorExecutionContext context)
        {
            TaskSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task") ?? throw new InvalidOperationException();
            TaskTSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1") ?? throw new InvalidOperationException();
            ValueTaskSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask") ?? throw new InvalidOperationException();
            ValueTaskTSymbol = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1") ?? throw new InvalidOperationException();
            VoidSymbol = context.Compilation.GetTypeByMetadataName("System.Void") ?? throw new InvalidOperationException();
            CancellationToken = context.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken") ?? throw new InvalidOperationException();
            DelegateSymbol = context.Compilation.GetTypeByMetadataName("System.Delegate") ?? throw new InvalidOperationException();
        }

        #endregion Public 方法
    }
}