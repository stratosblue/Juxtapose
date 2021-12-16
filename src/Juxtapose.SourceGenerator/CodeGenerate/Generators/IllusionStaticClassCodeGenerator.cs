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

        public JuxtaposeSourceGeneratorContext Context { get; }

        public IllusionStaticClassDescriptor Descriptor { get; }

        public SubResourceCollection Resources { get; }

        public string SourceHintName { get; }

        #endregion Public 属性

        #region Public 构造函数

        public IllusionStaticClassCodeGenerator(JuxtaposeSourceGeneratorContext context, IllusionStaticClassDescriptor descriptor, SubResourceCollection resources)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            Resources = resources ?? throw new ArgumentNullException(nameof(resources));
            SourceHintName = $"{Descriptor.TypeFullName}.StaticIllusion.g.cs";

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
                _sourceBuilder.AppendIndentLine($"/// <inheritdoc cref=\"{Descriptor.TargetType.ToFullyQualifiedDisplayString()}\"/>");
                _sourceBuilder.AppendIndentLine($"{Descriptor.Accessibility.ToCodeString()} static class {Descriptor.TypeName}");
                _sourceBuilder.Scope(() =>
                {
                    _sourceBuilder.AppendIndentLine($"private static readonly {Descriptor.ContextType.ToFullyQualifiedDisplayString()} s_context = {Descriptor.ContextType.ToFullyQualifiedDisplayString()}.SharedInstance;", true);

                    new StaticProxyCodeGenerator(Context, _sourceBuilder, Descriptor.TargetType, _vars).GenerateMemberProxyCode();
                });
            }, Descriptor.Namespace);
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
            var allProxyableMembers = Descriptor.TargetType.GetProxyableMembers(false).Where(m => m.DeclaredAccessibility == Accessibility.Public);

            #region 委托

            //HACK 暂不处理嵌套委托
            var delegateSymbols = allProxyableMembers.OfType<IMethodSymbol>()
                                                     .SelectMany(m => m.Parameters)
                                                     .Where(m => m.Type.IsDelegate())
                                                     .Select(m => m.Type)
                                                     .OfType<INamedTypeSymbol>()
                                                     .Select(m => m.DelegateInvokeMethod)
                                                     .Distinct(SymbolEqualityComparer.Default)
                                                     .OfType<IMethodSymbol>()
                                                     .ToArray();

            foreach (var item in MethodParameterPackCodeGenerateUtil.Generate(delegateSymbols, "Delegates.ParameterPack.g.cs"))
            {
                Resources.TryAddDelegateArgumentPackSourceCode(item);
                yield return item;
            }

            #endregion 委托

            foreach (var item in MethodParameterPackCodeGenerateUtil.Generate(allProxyableMembers, $"{Descriptor.TargetType.ToDisplayString()}.ParameterPack.g.cs"))
            {
                Resources.TryAddMethodArgumentPackSourceCode(item);
                yield return item;
            }

            yield return new FullSourceCode(SourceHintName, GenerateProxyTypeSource());
        }

        #endregion Public 方法
    }
}