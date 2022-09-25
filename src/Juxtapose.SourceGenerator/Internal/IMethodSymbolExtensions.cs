using System;
using System.Linq;
using System.Text;

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

    public static string GetResultPackClassName(this IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetNormalizeClassName() + "_ResultPack";
    }

    public static bool NotStatic(this IMethodSymbol methodSymbol)
    {
        return !methodSymbol.IsStatic;
    }

    #endregion Public 方法
}