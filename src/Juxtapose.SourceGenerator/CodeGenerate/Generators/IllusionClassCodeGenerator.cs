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

        public JuxtaposeSourceGeneratorContext Context { get; }

        public IllusionInstanceClassDescriptor Descriptor { get; }

        public SubResourceCollection Resources { get; }

        public string SourceHintName { get; }

        #endregion Public 属性

        #region Public 构造函数

        public IllusionClassCodeGenerator(JuxtaposeSourceGeneratorContext context, IllusionInstanceClassDescriptor descriptor, SubResourceCollection resources)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            Resources = resources ?? throw new ArgumentNullException(nameof(resources));
            SourceHintName = $"{Descriptor.TypeFullName}.Illusion.g.cs";

            _vars = new VariableName()
            {
                Executor = "Executor",
            };
        }

        #endregion Public 构造函数

        #region Private 方法

        #region Constructor

        private void GenerateConstructorCommonProxyCode()
        {
            _sourceBuilder.AppendLine($@"public {Descriptor.TypeName}({TypeFullNames.Juxtapose.IJuxtaposeExecutorOwner} executorOwner, int instanceId)
{{
    _executorOwner = executorOwner;
    _instanceId = instanceId;

    _executor = _executorOwner.Executor;

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

var executorOwner = await {Descriptor.ContextType.ToFullyQualifiedDisplayString()}.SharedInstance.GetExecutorOwnerAsync(s__creationContext, cancellation);
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

        private void GenerateConstructorProxyCode()
        {
            if (Context.Resources.IsProvideByServiceProvider(Descriptor.TargetType))
            {
                GenerateServiceProviderConstructorProxyCode();
            }
            else
            {
                GenerateNormalConstructorProxyCode();
            }

            GenerateConstructorCommonProxyCode();
        }

        private void GenerateNormalConstructorProxyCode()
        {
            var targetTypeName = Descriptor.TargetType.Name;
            var generatedTypeName = Descriptor.TypeFullName;
            foreach (var constructor in Descriptor.TargetType.Constructors.Where(m => m.NotStatic()))
            {
                if (!Context.TryGetConstructorParameterPackWithDiagnostic(constructor, generatedTypeName, out var parameterPackSourceCode))
                {
                    continue;
                }

                var paramPackContext = constructor.GetConstructorParamPackContext(generatedTypeName);

                var ctorAnnotation = $"/// <inheritdoc cref=\"{targetTypeName}.{targetTypeName}({string.Join(", ", constructor.Parameters.Select(m => m.Type.ToFullyQualifiedDisplayString()))})\"/>";

                var ctorArguments = constructor.GenerateMethodArgumentString();
                var accessibility = constructor.GetAccessibilityCodeString();

                _sourceBuilder.AppendIndentLine(ctorAnnotation);
                _sourceBuilder.AppendIndentLine($"[Obsolete(\"Use static method \\\"{Descriptor.TypeName}.NewAsync()\\\" instead of sync constructor.\")]");
                _sourceBuilder.AppendIndentLine($"{accessibility} {Descriptor.TypeName}({ctorArguments})");
                _sourceBuilder.Scope(() =>
                {
                    paramPackContext.GenParamPackCode(_sourceBuilder, "parameterPack");

                    _sourceBuilder.AppendLine(@"
var (executorOwner, instanceId) = CreateObjectAsync(parameterPack, true, CancellationToken.None).GetAwaiter().GetResult();

_executorOwner = executorOwner;
_instanceId = instanceId;

_executor = _executorOwner.Executor;

_runningTokenRegistration = _executorOwner.Executor.RunningToken.Register(Dispose);
_runningTokenSource = new CancellationTokenSource();
_runningToken = _runningTokenSource.Token;");
                });
                _sourceBuilder.AppendLine();

                _sourceBuilder.AppendIndentLine(ctorAnnotation);
                _sourceBuilder.AppendIndentLine($"{accessibility} static async Task<{Descriptor.TypeName}> NewAsync({ctorArguments}{(ctorArguments.Length > 0 ? ", " : string.Empty)}CancellationToken cancellation = default)");
                _sourceBuilder.Scope(() =>
                {
                    paramPackContext.GenParamPackCode(_sourceBuilder, "parameterPack");

                    _sourceBuilder.AppendLine($@"
var (executorOwner, instanceId) = await CreateObjectAsync(parameterPack, false, cancellation);
return new {Descriptor.TypeName}(executorOwner, instanceId);");
                });
                _sourceBuilder.AppendLine();
            }
        }

        private void GenerateServiceProviderConstructorProxyCode()
        {
            _sourceBuilder.AppendLine(@"/// <summary>
/// Create a object through <see cref=""IServiceProvider""/> in child process.
/// </summary>");

            _sourceBuilder.AppendIndentLine($"public static async Task<{Descriptor.TypeName}> NewAsync(CancellationToken cancellation = default)");
            _sourceBuilder.Scope(() =>
            {
                _sourceBuilder.AppendLine($@"
var parameterPack = new global::Juxtapose.Messages.ParameterPacks.ServiceProviderGetInstanceParameterPack(""{Descriptor.TypeFullName}"");
var (executorOwner, instanceId) = await CreateObjectAsync(parameterPack, false, cancellation);
return new {Descriptor.TypeName}(executorOwner, instanceId);");
            });

            _sourceBuilder.AppendLine();
        }

        #endregion Constructor

        private void GenerateProxyClassSource()
        {
            _sourceBuilder.AppendLine(Constants.JuxtaposeGenerateCodeHeader);
            _sourceBuilder.AppendLine();

            _sourceBuilder.Namespace(() =>
            {
                _sourceBuilder.AppendIndentLine($"/// <inheritdoc cref=\"{Descriptor.TargetType.ToFullyQualifiedDisplayString()}\"/>");

                var inheritBaseCodeSnippet = Descriptor.InheritType is null
                                             ? string.Empty
                                             : $"{Descriptor.InheritType.ToFullyQualifiedDisplayString()}, ";

                _sourceBuilder.AppendIndentLine($"{Descriptor.Accessibility.ToCodeString()} sealed partial class {Descriptor.TypeName} : {inheritBaseCodeSnippet}global::Juxtapose.IIllusion, {TypeFullNames.System.IDisposable}");

                _sourceBuilder.Scope(() =>
                {
                    _sourceBuilder.AppendIndentLine($"private static readonly {TypeFullNames.Juxtapose.ExecutorCreationContext} s__creationContext = new(typeof({Descriptor.TargetType.ToFullyQualifiedDisplayString()}), \"ctor\", false, true);", true);

                    _sourceBuilder.AppendLine($@"
private {TypeFullNames.Juxtapose.IJuxtaposeExecutorOwner} _executorOwner;

private {TypeFullNames.Juxtapose.JuxtaposeExecutor} {_vars.Executor} => _executorOwner.Executor;

private readonly int _instanceId;

private global::System.Threading.CancellationTokenSource _runningTokenSource;

private readonly global::System.Threading.CancellationToken _runningToken;

private CancellationTokenRegistration? _runningTokenRegistration;

#region IIllusion

private readonly global::Juxtapose.JuxtaposeExecutor _executor;

/// <inheritdoc/>
JuxtaposeExecutor IIllusion.Executor => _executor;

/// <inheritdoc/>
bool IIllusion.IsAvailable => !_isDisposed;

#endregion IIllusion

");
                    _sourceBuilder.AppendLine();

                    GenerateConstructorProxyCode();

                    new InstanceProxyCodeGenerator(Context, _sourceBuilder, Descriptor.InheritType ?? Descriptor.TargetType, new(_vars) { MethodBodyPrefixSnippet = "ThrowIfDisposed();" }).GenerateMemberProxyCode();

                    _sourceBuilder.AppendLine();

                    _sourceBuilder.AppendLine($@"private bool _isDisposed = false;

private void ThrowIfDisposed()
{{
    if (_isDisposed)
    {{
        throw new ObjectDisposedException(""_executorOwner"");
    }}
}}

~{Descriptor.TypeName}()
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
            var targetTypeSymbol = Descriptor.InheritType ?? Descriptor.TargetType;

            var allProxyableMembers = targetTypeSymbol.GetProxyableMembers(true).ToArray();

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

            #region 成员

            var targetTypeProxyableMembers = allProxyableMembers.Where(m => m is not IMethodSymbol methodSymbol || methodSymbol.MethodKind != MethodKind.Constructor)
                                                                .OfType<IMethodSymbol>()
                                                                .ToArray();

            foreach (var item in MethodParameterPackCodeGenerateUtil.Generate(targetTypeProxyableMembers, $"{targetTypeSymbol.ToDisplayString()}.ParameterPack.g.cs"))
            {
                Resources.TryAddMethodArgumentPackSourceCode(item);
                yield return item;
            }

            #endregion 成员

            #region 构造函数

            var constructors = Descriptor.TargetType.GetProxyableMembers(true)
                                                    .Where(m => m is IMethodSymbol methodSymbol && methodSymbol.MethodKind is MethodKind.Constructor)
                                                    .OfType<IMethodSymbol>()
                                                    .ToArray();

            foreach (var item in MethodParameterPackCodeGenerateUtil.GenerateConstructorPack(constructors, $"{targetTypeSymbol.ToDisplayString()}.ParameterPack.g.cs", Descriptor.TypeFullName))
            {
                Resources.TryAddConstructorParameterPackSourceCode(item);
                yield return item;
            }

            #endregion 构造函数

            yield return new FullSourceCode(SourceHintName, GenerateProxyTypeSource());
        }

        #endregion Public 方法
    }
}