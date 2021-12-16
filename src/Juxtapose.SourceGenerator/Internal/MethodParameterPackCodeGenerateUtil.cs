using System;
using System.Collections.Generic;
using System.Linq;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator
{
    public static class MethodParameterPackCodeGenerateUtil
    {
        #region Private 方法

        private static ConstructorParameterPackSourceCode GenConstructorParameterPackClassSource(IMethodSymbol methodSymbol, string sourceHintName, MethodParamPackContext packContext, string generatedTypeName)
        {
            return new ConstructorParameterPackSourceCode(methodSymbol,
                                                          sourceHintName,
                                                          packContext.ParamPackClassCode,
                                                          "Juxtapose.Messages.ParameterPacks.Generated",
                                                          packContext.ParamPackClassName,
                                                          $"Juxtapose.Messages.ParameterPacks.Generated.{packContext.ParamPackClassName}",
                                                          generatedTypeName);
        }

        private static ArgumentPackSourceCode GenMethodParameterPackClassSource(IMethodSymbol methodSymbol, string sourceHintName, MethodParamPackContext packContext)
        {
            return new ParameterPackSourceCode(methodSymbol,
                                               sourceHintName,
                                               packContext.ParamPackClassCode,
                                               "Juxtapose.Messages.ParameterPacks.Generated",
                                               packContext.ParamPackClassName,
                                               $"Juxtapose.Messages.ParameterPacks.Generated.{packContext.ParamPackClassName}");
        }

        private static ArgumentPackSourceCode GenMethodResultPackClassSource(IMethodSymbol methodSymbol, string sourceHintName, MethodParamPackContext packContext)
        {
            return new ResultPackSourceCode(methodSymbol,
                                            sourceHintName,
                                            packContext.ResultPackClassCode!,
                                            "Juxtapose.Messages.ParameterPacks.Generated",
                                            packContext.ResultPackClassName!,
                                            $"Juxtapose.Messages.ParameterPacks.Generated.{packContext.ResultPackClassName}");
        }

        #endregion Private 方法

        #region Public 方法

        public static IEnumerable<ArgumentPackSourceCode> Generate(IEnumerable<ISymbol> memberSymbols, string sourceHintName)
        {
            foreach (var methodSymbol in EnumerateMethods())
            {
                var packContext = methodSymbol.GetParamPackContext();
                yield return GenMethodParameterPackClassSource(methodSymbol, sourceHintName, packContext);
                if (methodSymbol.GetReturnType() is not null)
                {
                    yield return GenMethodResultPackClassSource(methodSymbol, sourceHintName, packContext);
                }
            }

            IEnumerable<IMethodSymbol> EnumerateMethods()
            {
                foreach (var memberSymbol in memberSymbols)
                {
                    switch (memberSymbol)
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
                            if (methodSymbol.MethodKind != MethodKind.PropertyGet
                                || methodSymbol.MethodKind != MethodKind.PropertySet)
                            {
                                yield return methodSymbol;
                            }
                            break;

                        default:
                            throw new NotSupportedException(memberSymbol.ToDisplayString());
                    };
                }
            }
        }

        public static IEnumerable<ConstructorParameterPackSourceCode> GenerateConstructorPack(IEnumerable<IMethodSymbol> methodSymbols, string sourceHintName, string generatedTypeName)
        {
            if (methodSymbols.Any(m => m.MethodKind != MethodKind.Constructor))
            {
                throw new ArgumentException("some method is not Constructor.", nameof(methodSymbols));
            }

            return InternalGenerate().OfType<ConstructorParameterPackSourceCode>();

            IEnumerable<ArgumentPackSourceCode> InternalGenerate()
            {
                foreach (var methodSymbol in methodSymbols)
                {
                    var packContext = methodSymbol.GetConstructorParamPackContext(generatedTypeName);
                    yield return GenConstructorParameterPackClassSource(methodSymbol, sourceHintName, packContext, generatedTypeName);
                }
            }
        }

        #endregion Public 方法
    }
}