using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Juxtapose.SourceGenerator.Model;

namespace Microsoft.CodeAnalysis
{
    internal static class IMethodSymbolExtensions
    {
        #region Private 字段

        private static readonly ConditionalWeakTable<IMethodSymbol, MethodParamPackContext> s_methodParamPackContextCache = new();

        #endregion Private 字段

        #region Public 方法

        public static string GenerateMethodArgumentString(this IMethodSymbol methodSymbol)
        {
            if (methodSymbol.Parameters.Length == 0)
            {
                return string.Empty;
            }
            return string.Join(", ", methodSymbol.Parameters.Select(m => $"{m.ToDisplayString()} {m.Name}"));
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

        public static string GetNormalizeClassName(this IMethodSymbol methodSymbol)
        {
            return Regex.Replace(methodSymbol.ToDisplayString(), "[^0-9a-zA-Z_]", "_");
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

        public static MethodParamPackContext GetParamPackContext(this IMethodSymbol methodSymbol)
        {
            if (!s_methodParamPackContextCache.TryGetValue(methodSymbol, out var packContext))
            {
                lock (s_methodParamPackContextCache)
                {
                    if (!s_methodParamPackContextCache.TryGetValue(methodSymbol, out packContext))
                    {
                        packContext = new MethodParamPackContext(methodSymbol);
                        s_methodParamPackContextCache.Add(methodSymbol, packContext);
                    }
                }
            }

            return packContext;
        }

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

        #endregion Public 方法
    }
}