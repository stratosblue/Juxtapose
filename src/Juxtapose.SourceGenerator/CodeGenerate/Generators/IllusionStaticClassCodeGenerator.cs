using System;
using System.Collections.Generic;
using System.Linq;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate
{
    public class IllusionStaticClassCodeGenerator : ISourceCodeProvider<SourceCode>
    {
        #region Private 字段

        private readonly ClassStringBuilder _sourceBuilder = new();

        private readonly VariableName _vars;

        private string? _generatedSource = null;

        #endregion Private 字段

        #region Public 属性

        public Accessibility Accessibility { get; }

        public JuxtaposeSourceGeneratorContext Context { get; }

        public INamedTypeSymbol ContextTypeSymbol { get; }

        public GeneratorExecutionContext GeneratorExecutionContext { get; }

        public string Namespace { get; }

        public string SourceHintName { get; }

        public INamedTypeSymbol StaticClassType { get; }

        public string TypeFullName { get; }

        public string TypeName { get; }

        #endregion Public 属性

        #region Public 构造函数

        public IllusionStaticClassCodeGenerator(JuxtaposeSourceGeneratorContext context, IllusionAttributeDefine attributeDefine, INamedTypeSymbol contextTypeSymbol)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ContextTypeSymbol = contextTypeSymbol ?? throw new ArgumentNullException(nameof(contextTypeSymbol));

            StaticClassType = attributeDefine.TargetType ?? throw new ArgumentNullException(nameof(attributeDefine.TargetType));

            var staticClassTypeFullName = StaticClassType.ToDisplayString();
            Namespace = staticClassTypeFullName.Substring(0, staticClassTypeFullName.LastIndexOf('.'));

            if (attributeDefine.GeneratedTypeName is not string proxyTypeName
                || string.IsNullOrWhiteSpace(proxyTypeName))
            {
                TypeFullName = $"{staticClassTypeFullName}Illusion";
                TypeName = TypeFullName.Substring(Namespace.Length + 1);
            }
            else
            {
                if (proxyTypeName.Contains('.'))
                {
                    TypeName = proxyTypeName.Substring(proxyTypeName.LastIndexOf('.') + 1);
                    TypeFullName = proxyTypeName;
                    Namespace = proxyTypeName.Substring(0, proxyTypeName.LastIndexOf('.'));
                }
                else
                {
                    TypeName = proxyTypeName;
                    TypeFullName = $"{Namespace}.{proxyTypeName}";
                }
            }

            Accessibility = attributeDefine.Accessibility switch
            {
                GeneratedAccessibility.InheritContext => contextTypeSymbol.DeclaredAccessibility,
                GeneratedAccessibility.Public => Accessibility.Public,
                GeneratedAccessibility.Internal => Accessibility.Internal,
                _ => StaticClassType.DeclaredAccessibility,
            };

            SourceHintName = $"{TypeFullName}.StaticIllusion.g.cs";

            _vars = new VariableName();
        }

        #endregion Public 构造函数

        #region Private 方法

        private void GenerateProxyClassSource()
        {
            _sourceBuilder.AppendLine(Constants.JuxtaposeGenerateCodeHeader);
            _sourceBuilder.AppendLine();
            _sourceBuilder.Namespace(() =>
            {
                _sourceBuilder.AppendIndentLine($"/// <inheritdoc cref=\"global::{StaticClassType.ToDisplayString()}\"/>");
                _sourceBuilder.AppendIndentLine($"{Accessibility.ToCodeString()} static class {TypeName}");
                _sourceBuilder.Scope(() =>
                {
                    _sourceBuilder.AppendIndentLine($"private static readonly global::{ContextTypeSymbol.ToDisplayString()} s_context = global::{ContextTypeSymbol.ToDisplayString()}.SharedInstance;", true);

                    new StaticProxyCodeGenerator(Context, _sourceBuilder, StaticClassType, _vars).GenerateMemberProxyCode();
                });
            }, Namespace);
        }

        private string GenerateProxyTypeSource()
        {
            if (_generatedSource != null)
            {
                return _generatedSource;
            }

            GenerateProxyClassSource();
            _generatedSource = _sourceBuilder.ToString();

            return _generatedSource;
        }

        #endregion Private 方法

        #region Public 方法

        public IEnumerable<SourceCode> GetSources()
        {
            var members = StaticClassType.GetProxyableMembers().Where(m => m.DeclaredAccessibility == Accessibility.Public);

            Context.TryAddStaticMethods(StaticClassType, members.GetMethodSymbols().Where(m => m.DeclaredAccessibility == Accessibility.Public));

            var parameterPackCodeGenerator = new MethodParameterPackCodeGenerator(Context, members, Namespace, $"{StaticClassType.ToDisplayString()}.ParameterPack.g.cs");
            var parameterPackTypeSources = parameterPackCodeGenerator.GetSources()
                                                                     .ToList();

            Context.TryAddMethodArgumentPackSourceCodes(parameterPackTypeSources);

            foreach (var parameterPackTypeSource in parameterPackTypeSources)
            {
                yield return parameterPackTypeSource;
            }

            yield return new FullSourceCode(SourceHintName, GenerateProxyTypeSource());
        }

        #endregion Public 方法
    }
}