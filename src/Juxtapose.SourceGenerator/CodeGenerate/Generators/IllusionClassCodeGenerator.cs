using System;
using System.Collections.Generic;
using System.Linq;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate
{
    public class IllusionClassCodeGenerator : ISourceCodeProvider<SourceCode>
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

        public INamedTypeSymbol ImplementTypeSymbol { get; }

        public INamedTypeSymbol InterfaceTypeSymbol { get; }

        public string Namespace { get; }

        public string SourceHintName { get; }

        public string TypeFullName { get; }

        public string TypeName { get; }

        #endregion Public 属性

        #region Public 构造函数

        public IllusionClassCodeGenerator(JuxtaposeSourceGeneratorContext context, IllusionAttributeDefine attributeDefine, INamedTypeSymbol contextTypeSymbol)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ContextTypeSymbol = contextTypeSymbol ?? throw new ArgumentNullException(nameof(contextTypeSymbol));

            ImplementTypeSymbol = attributeDefine.TargetType ?? throw new ArgumentNullException(nameof(attributeDefine.TargetType));
            InterfaceTypeSymbol = attributeDefine.InheritType ?? throw new ArgumentNullException(nameof(attributeDefine.InheritType));

            var implementTypeFullName = ImplementTypeSymbol.ToDisplayString();
            Namespace = implementTypeFullName.Substring(0, implementTypeFullName.LastIndexOf('.'));

            if (attributeDefine.GeneratedTypeName is not string proxyTypeName
                || string.IsNullOrWhiteSpace(proxyTypeName))
            {
                TypeFullName = $"{implementTypeFullName}As{InterfaceTypeSymbol.Name}Illusion";
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
                GeneratedAccessibility.InheritInterface => InterfaceTypeSymbol.DeclaredAccessibility,
                GeneratedAccessibility.Public => Accessibility.Public,
                GeneratedAccessibility.Internal => Accessibility.Internal,
                _ => ImplementTypeSymbol.DeclaredAccessibility,
            };

            SourceHintName = $"{TypeFullName}.g.cs";

            _vars = new VariableName()
            {
                Executor = "Executor",
            };
        }

        #endregion Public 构造函数

        #region Private 方法

        private void GenerateConstructorProxyCode()
        {
            var implTypeName = ImplementTypeSymbol.Name;
            foreach (var constructor in ImplementTypeSymbol.Constructors)
            {
                var parameterPackSourceCode = Context.MethodParameterPacks[constructor];
                var paramPackContext = constructor.GetParamPackContext();

                var ctorAnnotation = $"/// <inheritdoc cref=\"{implTypeName}.{implTypeName}({string.Join(", ", constructor.Parameters.Select(m => m.Type.ToDisplayString()))})\"/>";

                var ctorArguments = constructor.GenerateMethodArgumentString();
                var accessibility = constructor.GetAccessibilityCodeString();

                _sourceBuilder.AppendIndentLine(ctorAnnotation);
                _sourceBuilder.AppendIndentLine($"[Obsolete(\"Use static method \\\"{TypeName}.NewAsync()\\\" instead of sync constructor.\")]");
                _sourceBuilder.AppendIndentLine($"{accessibility} {TypeName}({ctorArguments})");
                _sourceBuilder.Scope(() =>
                {
                    paramPackContext.GenParamPackCode(_sourceBuilder, "parameterPack");

                    _sourceBuilder.AppendLine(@"
var (executorOwner, instanceId) = CreateObjectAsync(parameterPack, true, CancellationToken.None).GetAwaiter().GetResult();

_executorOwner = executorOwner;
_instanceId = instanceId;

_runningTokenRegistration = _executorOwner.Executor.RunningToken.Register(Dispose);
_runningTokenSource = new CancellationTokenSource();
_runningToken = _runningTokenSource.Token;");
                });
                _sourceBuilder.AppendLine();

                _sourceBuilder.AppendIndentLine(ctorAnnotation);
                _sourceBuilder.AppendIndentLine($"{accessibility} static async Task<{TypeName}> NewAsync({ctorArguments}{(ctorArguments.Length > 0 ? ", " : string.Empty)}CancellationToken cancellation = default)");
                _sourceBuilder.Scope(() =>
                {
                    paramPackContext.GenParamPackCode(_sourceBuilder, "parameterPack");

                    _sourceBuilder.AppendLine($@"
var (executorOwner, instanceId) = await CreateObjectAsync(parameterPack, false, cancellation);
return new {TypeName}(executorOwner, instanceId);");
                });
                _sourceBuilder.AppendLine();
            }

            _sourceBuilder.AppendLine($@"public {TypeName}({TypeFullNames.Juxtapose.IJuxtaposeExecutorOwner} executorOwner, int instanceId)
{{
    _executorOwner = executorOwner;
    _instanceId = instanceId;
    _runningTokenRegistration = _executorOwner.Executor.RunningToken.Register(Dispose);
    _runningTokenSource = new CancellationTokenSource();
    _runningToken = _runningTokenSource.Token;
}}");

            _sourceBuilder.AppendLine();

            _sourceBuilder.AppendLine($"private static async {TypeFullNames.System.Threading.Tasks.Task}<({TypeFullNames.Juxtapose.IJuxtaposeExecutorOwner} executorOwner, int instanceId)> CreateObjectAsync<TParameterPack>(TParameterPack parameterPack, bool noContext = true, {TypeFullNames.System.Threading.CancellationToken} cancellation = default) where TParameterPack : class");
            _sourceBuilder.Scope(() =>
            {
                _sourceBuilder.AppendLine($@"if (noContext)
{{
    await global::Juxtapose.SynchronizationContextRemover.Awaiter;
}}

var executorOwner = await global::{ContextTypeSymbol.ToDisplayString()}.SharedInstance.GetExecutorOwnerAsync(s__creationContext, cancellation);
var executor = executorOwner.Executor;

var instanceId = executor.InstanceIdGenerator.Next();
var message = new global::Juxtapose.Messages.CreateObjectInstanceMessage<TParameterPack>(instanceId) {{ ParameterPack = parameterPack }};

try
{{
    await executorOwner.Executor.InvokeMessageAsync(message, cancellation);
}}
catch
{{
    executorOwner.Dispose();
    throw;
}}

return (executorOwner, instanceId);");
            });
        }

        private void GenerateProxyClassSource()
        {
            _sourceBuilder.AppendLine(Constants.JuxtaposeGenerateCodeHeader);
            _sourceBuilder.AppendLine();

            _sourceBuilder.Namespace(() =>
            {
                _sourceBuilder.AppendIndentLine($"/// <inheritdoc cref=\"global::{ImplementTypeSymbol.ToDisplayString()}\"/>");
                _sourceBuilder.AppendIndentLine($"{Accessibility.ToCodeString()} sealed partial class {TypeName} : global::{InterfaceTypeSymbol.ToDisplayString()}, {TypeFullNames.System.IDisposable}");

                _sourceBuilder.Scope(() =>
                {
                    _sourceBuilder.AppendIndentLine($"private static readonly {TypeFullNames.Juxtapose.ExecutorCreationContext} s__creationContext = new (typeof(global::{ImplementTypeSymbol.ToDisplayString()}), \"ctor\", false, true);", true);
                    _sourceBuilder.AppendIndentLine($"private {TypeFullNames.Juxtapose.IJuxtaposeExecutorOwner} _executorOwner;", true);
                    _sourceBuilder.AppendIndentLine($"private {TypeFullNames.Juxtapose.JuxtaposeExecutor} {_vars.Executor} => _executorOwner.Executor;", true);
                    _sourceBuilder.AppendIndentLine("private readonly int _instanceId;", true);
                    _sourceBuilder.AppendIndentLine("private global::System.Threading.CancellationTokenSource _runningTokenSource;", true);
                    _sourceBuilder.AppendIndentLine("private readonly global::System.Threading.CancellationToken _runningToken;", true);
                    _sourceBuilder.AppendIndentLine("private CancellationTokenRegistration? _runningTokenRegistration;", true);

                    GenerateConstructorProxyCode();

                    new InstanceProxyCodeGenerator(Context, _sourceBuilder, InterfaceTypeSymbol, new(_vars) { MethodBodyPrefixSnippet = "ThrowIfDisposed();" }).GenerateMemberProxyCode();

                    _sourceBuilder.AppendLine();

                    _sourceBuilder.AppendLine($@"private bool _isDisposed = false;

private void ThrowIfDisposed()
{{
    if (_isDisposed)
    {{
        throw new ObjectDisposedException(""_executorOwner"");
    }}
}}

~{TypeName}()
{{
    Dispose();
}}

public void Dispose()
{{
    if (_isDisposed)
    {{
        return;
    }}
    _isDisposed = true;
    {_vars.Executor}.DisposeObjectInstance(_instanceId);
    _runningTokenSource.Cancel();
    _runningTokenSource.Dispose();
    _executorOwner.Dispose();
    _runningTokenRegistration?.Dispose();
    _executorOwner = null!;
    _runningTokenSource = null!;
    _runningTokenRegistration = null;
    global::System.GC.SuppressFinalize(this);
}}");
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
            //HACK 暂不处理嵌套委托
            var delegateSymbols = InterfaceTypeSymbol.GetProxyableMembers()
                                                     .OfType<IMethodSymbol>()
                                                     .SelectMany(m => m.Parameters)
                                                     .Where(m => m.Type.IsDelegate())
                                                     .Select(m => m.Type)
                                                     .OfType<INamedTypeSymbol>()
                                                     .Select(m => m.DelegateInvokeMethod)
                                                     .OfType<IMethodSymbol>()
                                                     .ToArray();

            var members = InterfaceTypeSymbol.GetProxyableMembers().Concat(ImplementTypeSymbol.Constructors).Concat(delegateSymbols);

            Context.TryAddInterfaceMethods(InterfaceTypeSymbol, InterfaceTypeSymbol.GetProxyableMembers().GetMethodSymbols());

            var parameterPackCodeGenerator = new MethodParameterPackCodeGenerator(Context, members, Namespace, $"{InterfaceTypeSymbol.ToDisplayString()}.ParameterPack.g.cs");
            var parameterPackTypeSources = parameterPackCodeGenerator.GetSources()
                                                                     .ToList();

            Context.TryAddConstructorMethods(ImplementTypeSymbol, ImplementTypeSymbol.Constructors);

            Context.TryAddMethodArgumentPackSourceCodes(parameterPackTypeSources);

            foreach (var parameterPackTypeSource in parameterPackTypeSources)
            {
                yield return parameterPackTypeSource;
            }

            Context.TryAddInterfaceImplement(InterfaceTypeSymbol, ImplementTypeSymbol);

            yield return new FullSourceCode(SourceHintName, GenerateProxyTypeSource());
        }

        #endregion Public 方法
    }
}