using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Juxtapose.SourceGenerator.Model;

namespace Microsoft.CodeAnalysis;

internal static class IMethodSymbolExtensions
{
    #region Public 方法

    public static string GenerateMethodArgumentString(this IMethodSymbol methodSymbol)
    {
        if (methodSymbol.Parameters.Length == 0)
        {
            return string.Empty;
        }
        return string.Join(", ", methodSymbol.Parameters.Select(Processparameter));

        static string Processparameter(IParameterSymbol parameterSymbol)
        {
            //TODO check int out ref ...

            var builder = new StringBuilder(128);
            var displayString = parameterSymbol.Type.ToFullyQualifiedDisplayString();

            builder.Append(displayString);
            builder.Append(' ');
            builder.Append(parameterSymbol.Name);
            builder.Append(GetDefaultValueExpression(parameterSymbol));

            return builder.ToString();
        }

        static string GetDefaultValueExpression(IParameterSymbol parameterSymbol)
        {
            //TODO check params
            if (!parameterSymbol.HasExplicitDefaultValue)
            {
                return string.Empty;
            }

            var value = parameterSymbol.ExplicitDefaultValue;

            if (value is null)
            {
                return parameterSymbol.Type.IsValueType
                       ? " = default"
                       : " = null";
            }

#pragma warning disable IDE0071 // 简化内插

            return value is string
                   ? $" = \"{value}\""
                   : $" = {value.ToString()}";

#pragma warning restore IDE0071 // 简化内插
        }
    }

    public static string GenerateMethodArgumentStringWithoutType(this IMethodSymbol methodSymbol)
    {
        if (methodSymbol is null
            || methodSymbol.Parameters.Length == 0)
        {
            return string.Empty;
        }
        return string.Join(", ", methodSymbol.Parameters.Select(m => m.Name));
    }

    public static string GetCreationContextVariableName(this IMethodSymbol methodSymbol) => $"s__context_{methodSymbol.GetIdentifier()}";

    public static string GetNormalizeClassName(this ISymbol symbol)
    {
        return symbol.ToDisplayString().NormalizeAsClassName();
    }

    public static string GetParamPackClassName(this IMethodSymbol methodSymbol)
    {
        if (methodSymbol.MethodKind == MethodKind.Constructor)
        {
            return methodSymbol.GetNormalizeClassName() + "_ctor_ParamPack";
        }
        else
        {
            return methodSymbol.GetNormalizeClassName() + "_ParamPack";
        }
    }

    #region GetParamPackContext

    private static readonly ConditionalWeakTable<IMethodSymbol, Dictionary<string, ConstructorParamPackContext>> s_constructorParamPackContextCache = new();
    private static readonly ConditionalWeakTable<IMethodSymbol, MethodParamPackContext> s_methodParamPackContextCache = new();

    public static ConstructorParamPackContext GetConstructorParamPackContext(this IMethodSymbol methodSymbol, string generatedTypeName)
    {
        if (methodSymbol.MethodKind != MethodKind.Constructor)
        {
            throw new ArgumentException($"{methodSymbol.ToDisplayString()} not a constructor.", nameof(methodSymbol));
        }

        if (string.IsNullOrWhiteSpace(generatedTypeName))
        {
            throw new ArgumentException($"“{nameof(generatedTypeName)}”不能为 null 或空白。", nameof(generatedTypeName));
        }

        lock (s_constructorParamPackContextCache)
        {
            if (s_constructorParamPackContextCache.TryGetValue(methodSymbol, out var packContextMap))
            {
                if (!packContextMap.TryGetValue(generatedTypeName, out var packContext))
                {
                    packContext = new ConstructorParamPackContext(methodSymbol, generatedTypeName);
                    packContextMap.Add(generatedTypeName, packContext);
                }
                return packContext;
            }
            else
            {
                var packContext = new ConstructorParamPackContext(methodSymbol, generatedTypeName);
                s_constructorParamPackContextCache.Add(methodSymbol, new() { { generatedTypeName, packContext } });
                return packContext;
            }
        }
    }

    public static MethodParamPackContext GetParamPackContext(this IMethodSymbol methodSymbol)
    {
        if (methodSymbol.MethodKind == MethodKind.Constructor)
        {
            throw new ArgumentException($"{methodSymbol.ToDisplayString()} is a constructor.", nameof(methodSymbol));
        }

        lock (s_methodParamPackContextCache)
        {
            if (!s_methodParamPackContextCache.TryGetValue(methodSymbol, out var packContext))
            {
                if (!s_methodParamPackContextCache.TryGetValue(methodSymbol, out packContext))
                {
                    packContext = new MethodParamPackContext(methodSymbol);
                    s_methodParamPackContextCache.Add(methodSymbol, packContext);
                }
            }
            return packContext;
        }
    }

    #endregion GetParamPackContext

    public static string GetResultPackClassName(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetNormalizeClassName() + "_ResultPack";
    }

    /// <summary>
    /// 获取方法的返回类型（当返回类型为Task`T`时，返回T的类型）当类型为 void 时，返回null
    /// </summary>
    /// <param name="methodSymbol"></param>
    /// <returns></returns>
    public static ITypeSymbol? GetReturnType(this IMethodSymbol methodSymbol)
    {
        var returnType = methodSymbol.ReturnType;
        if (returnType is INamedTypeSymbol namedTypeSymbol
            && (namedTypeSymbol.IsTaskT() || namedTypeSymbol.IsValueTaskT()))
        {
            return namedTypeSymbol.TypeArguments[0];
        }
        return returnType.IsVoid() || returnType.IsTask() || returnType.IsValueTask() ? null : returnType;
    }

    public static bool IsReturnVoidOrTask(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.ReturnsVoid
               || methodSymbol.ReturnType.IsTask()
               || methodSymbol.ReturnType.IsValueTask()
               ;
    }

    public static bool NotStatic(this IMethodSymbol methodSymbol)
    {
        return !methodSymbol.IsStatic;
    }

    #endregion Public 方法
}