using System;
using System.Collections.Generic;
using System.Linq;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate
{
    public class MethodParameterPackCodeGenerator : ISourceCodeProvider<ArgumentPackSourceCode>
    {
        #region Public 属性

        public JuxtaposeSourceGeneratorContext Context { get; }

        public ISymbol[] MemberSymbols { get; }

        public string Namespace { get; set; }

        public string SourceHintName { get; }

        #endregion Public 属性

        #region Public 构造函数

        public MethodParameterPackCodeGenerator(JuxtaposeSourceGeneratorContext context,
                                                IEnumerable<ISymbol> memberSymbols,
                                                string @namespace,
                                                string sourceHintName)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
            {
                throw new ArgumentException($"“{nameof(@namespace)}”不能为 null 或空白。", nameof(@namespace));
            }

            if (string.IsNullOrWhiteSpace(sourceHintName))
            {
                throw new ArgumentException($"“{nameof(sourceHintName)}”不能为 null 或空白。", nameof(sourceHintName));
            }

            Context = context ?? throw new ArgumentNullException(nameof(context));
            MemberSymbols = memberSymbols?.ToArray() ?? throw new ArgumentNullException(nameof(memberSymbols));

            Namespace = @namespace;
            SourceHintName = sourceHintName;
        }

        #endregion Public 构造函数

        #region Private 方法

        private ArgumentPackSourceCode? GenMethodParameterPackClassSource(IMethodSymbol? methodSymbol)
        {
            if (methodSymbol is null
                || Context.MethodParameterPacks.ContainsKey(methodSymbol))
            {
                return null;
            }
            var packContext = methodSymbol.GetParamPackContext();
            var source = new ParameterPackSourceCode(methodSymbol,
                                                     SourceHintName,
                                                     packContext.ParamPackClassCode,
                                                     "Juxtapose.Messages.ParameterPacks.Generated",
                                                     packContext.ParamPackClassName,
                                                     $"{Namespace}.{packContext.ParamPackClassName}");
            return source;
        }

        private ArgumentPackSourceCode? GenMethodResultPackClassSource(IMethodSymbol? methodSymbol)
        {
            if (methodSymbol is null)
            {
                return null;
            }

            if (methodSymbol.GetReturnType() is null)
            {
                Context.MethodResultPacks[methodSymbol] = null;
            }
            else
            {
                if (Context.MethodResultPacks.ContainsKey(methodSymbol))
                {
                    return null;
                }
                var packContext = methodSymbol.GetParamPackContext();
                var source = new ResultPackSourceCode(methodSymbol,
                                                      SourceHintName,
                                                      packContext.ResultPackClassCode!,
                                                      "Juxtapose.Messages.ParameterPacks.Generated",
                                                      packContext.ResultPackClassName!,
                                                      $"{Namespace}.{packContext.ResultPackClassName}");
                return source;
            }
            return null;
        }

        private IEnumerable<ArgumentPackSourceCode?> InternalGetSources()
        {
            foreach (var member in MemberSymbols)
            {
                switch (member)
                {
                    case IPropertySymbol propertySymbol:
                        yield return GenMethodParameterPackClassSource(propertySymbol.SetMethod);
                        yield return GenMethodResultPackClassSource(propertySymbol.GetMethod);
                        break;

                    case IMethodSymbol methodSymbol:
                        if (methodSymbol.MethodKind != MethodKind.PropertyGet
                            || methodSymbol.MethodKind != MethodKind.PropertySet)
                        {
                            yield return GenMethodParameterPackClassSource(methodSymbol);
                            yield return GenMethodResultPackClassSource(methodSymbol);
                        }
                        break;

                    default:
                        throw new NotSupportedException(member.ToDisplayString());
                };
            }
        }

        #endregion Private 方法

        #region Public 方法

        public IEnumerable<ArgumentPackSourceCode> GetSources()
        {
            return InternalGetSources().OfType<ArgumentPackSourceCode>();
        }

        #endregion Public 方法
    }
}