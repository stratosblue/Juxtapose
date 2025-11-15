using Juxtapose.SourceGenerator.Internal;
using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate;

public class IllusionClassCodeGenerator : ISourceCodeProvider<SourceCode>
{
    #region Private 字段

    private readonly ClassStringBuilder _sourceBuilder = new();

    private readonly VariableName _vars;

    private string? _generatedSource = null;

    #endregion Private 字段

    #region Public 属性

    public JuxtaposeContextSourceGeneratorContext Context { get; }

    public IllusionInstanceClassDescriptor Descriptor { get; }

    public SubResourceCollection Resources { get; }

    public string SourceHintName { get; }

    #endregion Public 属性

    #region Public 构造函数

    public IllusionClassCodeGenerator(JuxtaposeContextSourceGeneratorContext context, IllusionInstanceClassDescriptor descriptor, SubResourceCollection resources)
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
    {_vars.InstanceId} = instanceId;

    _executor = _executorOwner.Executor;

    _runningTokenRegistration = _executorOwner.Executor.RunningToken.Register(Dispose);
    _runningTokenSource = new CancellationTokenSource();
    {_vars.RunningToken} = _runningTokenSource.Token;
}}");

        _sourceBuilder.AppendLine();

        _sourceBuilder.AppendLine($"private static async {TypeFullNames.System.Threading.Tasks.Task}<({TypeFullNames.Juxtapose.IJuxtaposeExecutorOwner} executorOwner, int instanceId)> CreateObjectAsync<TParameterPack>(TParameterPack parameterPack, int commandId, bool noContext = true, {TypeFullNames.System.Threading.CancellationToken} cancellation = default)");
        _sourceBuilder.Scope(() =>
        {
            _sourceBuilder.AppendLine($@"if (noContext)
{{
    await global::Juxtapose.SynchronizationContextRemover.Awaiter;
}}

var executorOwner = await {Descriptor.ContextType.ToFullyQualifiedDisplayString()}.SharedInstance.GetExecutorOwnerAsync(s__creationContext, cancellation);
var executor = executorOwner.Executor;

var instanceId = executor.InstanceIdGenerator.Next();
var message = new global::Juxtapose.Messages.CreateObjectInstanceMessage<TParameterPack>(instanceId, commandId) {{ ParameterPack = parameterPack }};

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
        if (Descriptor.FromIoCContainer)
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
        foreach (var constructor in Resources.GetAllConstructors().Where(m => m.NotStatic()))
        {
            var ctorAnnotation = $"""
                        /// <summary>
                        /// <inheritdoc cref="{targetTypeName}.{targetTypeName}({string.Join(", ", constructor.Parameters.Select(m => m.Type.ToInheritDocCrefString()))})"/>
                        /// <br/><br/>generated from <see cref="{Descriptor.TargetType.ToInheritDocCrefString()}"/>
                        /// </summary>
                """;

            var commandId = Context.Resources.GetCommandId(constructor);

            var ctorArguments = constructor.GenerateMethodArgumentString();
            var accessibility = constructor.GetAccessibilityCodeString();

            _sourceBuilder.AppendIndentLine(ctorAnnotation);
            _sourceBuilder.AppendIndentLine($"[Obsolete(\"Use static method \\\"{Descriptor.TypeName}.NewAsync()\\\" instead of sync constructor.\")]");
            _sourceBuilder.AppendIndentLine($"{accessibility} {Descriptor.TypeName}({ctorArguments})");
            _sourceBuilder.Scope(() =>
            {
                ArgumentsAndResultsHelper.GenerateMethodArgumentsPackCode(constructor, Context.TypeSymbolAnalyzer, _sourceBuilder, "parameterPack");

                _sourceBuilder.AppendLine(@$"
var (executorOwner, instanceId) = CreateObjectAsync(parameterPack, (int){GeneratedCommandUtil.GetAccessExpression(Descriptor.ContextType, commandId)}, true, CancellationToken.None).GetAwaiter().GetResult();

_executorOwner = executorOwner;
{_vars.InstanceId} = instanceId;

_executor = _executorOwner.Executor;

_runningTokenRegistration = _executorOwner.Executor.RunningToken.Register(Dispose);
_runningTokenSource = new CancellationTokenSource();
{_vars.RunningToken} = _runningTokenSource.Token;");
            });
            _sourceBuilder.AppendLine();

            _sourceBuilder.AppendIndentLine(ctorAnnotation);
            _sourceBuilder.AppendIndentLine($"{accessibility} static async Task<{Descriptor.TypeName}> NewAsync({ctorArguments}{(ctorArguments.Length > 0 ? ", " : string.Empty)}CancellationToken cancellation = default)");
            _sourceBuilder.Scope(() =>
            {
                ArgumentsAndResultsHelper.GenerateMethodArgumentsPackCode(constructor, Context.TypeSymbolAnalyzer, _sourceBuilder, "parameterPack");

                _sourceBuilder.AppendLine($@"
var (executorOwner, instanceId) = await CreateObjectAsync(parameterPack, (int){GeneratedCommandUtil.GetAccessExpression(Descriptor.ContextType, commandId)}, false, cancellation);
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
var (executorOwner, instanceId) = await CreateObjectAsync(parameterPack, (int){TypeFullNames.Juxtapose.SpecialCommand}.{nameof(SpecialCommand.GetInstanceByServiceProvider)}, false, cancellation);
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
            var classAnnotation = $"""
                    /// <summary>
                    /// <inheritdoc cref="{Descriptor.TargetType.ToInheritDocCrefString()}"/>
                    /// <br/><br/>generated from <see cref="{Descriptor.TargetType.ToInheritDocCrefString()}"/>
                    /// </summary>
                """;
            _sourceBuilder.AppendIndentLine(classAnnotation);

            _sourceBuilder.AppendIndentLine($"{Descriptor.Accessibility.ToCodeString()} sealed partial class {Descriptor.TypeName} : global::Juxtapose.IIllusion, {TypeFullNames.System.IDisposable}");

            _sourceBuilder.Scope(() =>
            {
                _sourceBuilder.AppendIndentLine($"private static readonly {TypeFullNames.Juxtapose.ExecutorCreationContext} s__creationContext = new(typeof({Descriptor.TargetType.ToFullyQualifiedDisplayString()}), \"ctor\", false, true);", true);

                _sourceBuilder.AppendLine($@"
private {TypeFullNames.Juxtapose.IJuxtaposeExecutorOwner} _executorOwner;

private {TypeFullNames.Juxtapose.JuxtaposeExecutor} {_vars.Executor} => _executorOwner.Executor;

private readonly int {_vars.InstanceId};

private global::System.Threading.CancellationTokenSource _runningTokenSource;

private readonly global::System.Threading.CancellationToken {_vars.RunningToken};

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

                new InstanceProxyCodeGenerator(Context, Resources, _sourceBuilder, Descriptor.TargetType, new(_vars) { MethodBodyPrefixSnippet = "ThrowIfDisposed();" }).GenerateMemberProxyCode();

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
    {_vars.Executor}.DisposeObjectInstance({_vars.InstanceId});
    _runningTokenSource.Cancel();
    _runningTokenSource.Dispose();
    _executorOwner.Dispose();
    _runningTokenRegistration?.Dispose();
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
        yield return new FullSourceCode(SourceHintName, GenerateProxyTypeSource());
    }

    #endregion Public 方法
}
