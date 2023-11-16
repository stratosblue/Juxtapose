using Juxtapose.SourceGenerator.Internal;
using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate;

public class JuxtaposeExecutorCodeGenerator : ISourceCodeProvider<SourceCode>
{
    #region Private 字段

    private readonly ClassStringBuilder _sourceBuilder = new();

    private readonly VariableName _vars;

    private string? _generatedSource = null;

    #endregion Private 字段

    #region Public 属性

    public JuxtaposeContextSourceGeneratorContext Context { get; }

    /// <summary>
    /// 上下文类型符号
    /// </summary>
    public INamedTypeSymbol ContextTypeSymbol { get; }

    /// <summary>
    /// 上下文的命名空间
    /// </summary>
    public string? Namespace { get; }

    public string SourceHintName { get; }

    /// <summary>
    /// 上下文的类型完全名称
    /// </summary>
    public string TypeFullName { get; }

    #endregion Public 属性

    #region Public 构造函数

    public JuxtaposeExecutorCodeGenerator(JuxtaposeContextSourceGeneratorContext context, INamedTypeSymbol contextTypeSymbol)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        ContextTypeSymbol = contextTypeSymbol ?? throw new ArgumentNullException(nameof(contextTypeSymbol));
        TypeFullName = contextTypeSymbol.ToDisplayString();

        Namespace = contextTypeSymbol.ContainingNamespace.GetNamespaceName();

        SourceHintName = $"{TypeFullName}.Executor.g.cs";

        _vars = new VariableName()
        {
            Executor = "this",
        };
    }

    #endregion Public 构造函数

    #region Private 方法

    private string GeneratePartialContextExecutorSource()
    {
        if (_generatedSource != null)
        {
            return _generatedSource;
        }

        _sourceBuilder.AppendLine(Constants.JuxtaposeGenerateCodeHeader);
        _sourceBuilder.AppendLine();

        _sourceBuilder.Namespace(() =>
        {
            _sourceBuilder.AppendIndentLine($"partial class {ContextTypeSymbol.Name} : {TypeFullNames.Juxtapose.JuxtaposeContext}");
            _sourceBuilder.Scope(() =>
            {
                _sourceBuilder.AppendIndentLine($"protected class InternalJuxtaposeExecutor : {TypeFullNames.Juxtapose.JuxtaposeExecutor}");
                _sourceBuilder.Scope(() =>
                {
                    _sourceBuilder.AppendIndentLine($"public InternalJuxtaposeExecutor({TypeFullNames.Juxtapose.IMessageExchanger} messageExchanger, global::Microsoft.Extensions.Logging.ILogger logger, global::System.Func<ValueTask<IIoCContainerHolder>>? iocContainerHolderGetter = null) : base(messageExchanger, logger, iocContainerHolderGetter) {{ }}", true);

                    _sourceBuilder.AppendIndentLine($"protected override async {TypeFullNames.System.Threading.Tasks.Task}<{TypeFullNames.Juxtapose.Messages.JuxtaposeMessage}?> OnMessageAsync({TypeFullNames.Juxtapose.Messages.JuxtaposeMessage} {_vars.Message}, {TypeFullNames.System.Threading.CancellationToken} __cancellation__)");
                    _sourceBuilder.Scope(() =>
                    {
                        _sourceBuilder.AppendIndentLine($"if ({_vars.Message} is {TypeFullNames.Juxtapose.JuxtaposeCommandMessage} @___cmd_message_)");
                        _sourceBuilder.Scope(() =>
                        {
                            _sourceBuilder.AppendIndentLine("switch (@___cmd_message_.CommandId)");
                            _sourceBuilder.Scope(() =>
                            {
                                GenerateAllServiceProviderObjectConstructorProcessCode();

                                GenerateAllObjectConstructorProcessCode();

                                GenerateAllStaticMethodProcessCode();

                                _sourceBuilder.AppendLine();
                            });
                        });

                        _sourceBuilder.AppendLine();
                        _sourceBuilder.AppendIndentLine($"return await base.OnMessageAsync({_vars.Message}, __cancellation__);");
                    });
                });
            });
        }, Namespace);

        _generatedSource = _sourceBuilder.ToString();

        return _generatedSource;
    }

    #region Generate

    #region ObjectConstructor

    private void GenerateAllObjectConstructorProcessCode()
    {
        var targetTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var item in Context.IllusionInstanceClasses.Where(m => !m.Key.FromIoCContainer))
        {
            if (!targetTypes.Contains(item.Key.TargetType))
            {
                GenerateConstructorProcessCode(item.Key, item.Value);
                targetTypes.Add(item.Key.TargetType);
            }
        }
    }

    private void GenerateAllServiceProviderObjectConstructorProcessCode()
    {
        var fromServiceProviderDescriptorMaps = Context.IllusionInstanceClasses.Where(m => m.Key.FromIoCContainer).ToArray();

        if (fromServiceProviderDescriptorMaps.Length == 0)
        {
            return;
        }

        _sourceBuilder.AppendIndentLine($"case (int){TypeFullNames.Juxtapose.SpecialCommand}.{nameof(SpecialCommand.GetInstanceByServiceProvider)}:");
        _sourceBuilder.Indent();
        _sourceBuilder.AppendIndentLine("{");
        _sourceBuilder.Indent();

        _sourceBuilder.AppendIndentLine($"var createObjectByServiceProviderMessage = (global::Juxtapose.Messages.CreateObjectInstanceMessage<ServiceProviderGetInstanceParameterPack>){_vars.Message};");
        _sourceBuilder.AppendIndentLine("var instanceId = createObjectByServiceProviderMessage.InstanceId;");
        _sourceBuilder.AppendIndentLine("IMessageExecutor realObjectInvoker = createObjectByServiceProviderMessage?.ParameterPack?.TypeFullName switch");
        _sourceBuilder.AppendIndentLine("{");
        _sourceBuilder.Indent();

        foreach (var descriptorMap in fromServiceProviderDescriptorMaps)
        {
            var descriptor = descriptorMap.Key;
            if (!descriptorMap.Value.TryGetRealObjectInvokerSourceCode(descriptor.TargetType, out var invokerSourceCode)
                || invokerSourceCode is null)
            {
                Context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ExecutorGenerateCanNotFoundGeneratedRealObjectInvoker, null, descriptor.TargetType, null));
                continue;
            }
            _sourceBuilder.AppendIndentLine($"\"{descriptor.TypeFullName}\" => new global::{invokerSourceCode.TypeFullName}(GetRequiredService<{descriptor.TargetType.ToFullyQualifiedDisplayString()}>(), instanceId),");
        }

        _sourceBuilder.AppendIndentLine("_ => throw new InvalidOperationException($\"Can not get instance of type {createObjectByServiceProviderMessage?.ParameterPack?.TypeFullName} by service provider. There is no code map for it.\"),");

        _sourceBuilder.Dedent();
        _sourceBuilder.AppendIndentLine("};");

        _sourceBuilder.AppendLine(@"AddObjectInstance(instanceId, realObjectInvoker);
return null;");

        _sourceBuilder.Dedent();
        _sourceBuilder.AppendIndentLine("}");
        _sourceBuilder.Dedent();
    }

    private void GenerateConstructorProcessCode(IllusionInstanceClassDescriptor descriptor, ResourceCollection resources)
    {
        var vars = new VariableName(_vars)
        {
            ParameterPack = "typedMessage.ParameterPack!",
        };

        if (!resources.TryGetRealObjectInvokerSourceCode(descriptor.TargetType, out var realObjectInvokerSourceCode)
            || realObjectInvokerSourceCode is null)
        {
            Context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ExecutorGenerateCanNotFoundGeneratedRealObjectInvoker, null, descriptor.TargetType, null));
            return;
        }

        var realObjectInvokerTypeFullName = realObjectInvokerSourceCode.TypeFullName;

        foreach (var constructorSymbol in resources.GetAllConstructors())
        {
            var commandId = resources.GetCommandId(constructorSymbol);

            _sourceBuilder.AppendIndentLine($"case (int){Context.GetCommandAccessExpression(commandId)}:");

            _sourceBuilder.Indent();
            _sourceBuilder.Scope(() =>
            {
                var methodInvokeMessageTypeName = $"{TypeFullNames.Juxtapose.Messages.CreateObjectInstanceMessage}<{ArgumentsAndResultsHelper.GenerateArgumentTypeName(constructorSymbol, Context.TypeSymbolAnalyzer)}>";

                _sourceBuilder.AppendIndentLine($"var typedMessage = ({methodInvokeMessageTypeName}){_vars.Message};");
                _sourceBuilder.AppendIndentLine($"var instanceId = typedMessage.InstanceId;");

                ArgumentsAndResultsHelper.GenerateMethodArgumentsUnpackCode(constructorSymbol, Context, resources, _sourceBuilder, () =>
                {
                    _sourceBuilder.AppendIndentLine($"var instance = new {constructorSymbol.ContainingType.ToFullyQualifiedDisplayString()}({constructorSymbol.GenerateMethodArgumentStringWithoutType()});");

                    _sourceBuilder.AppendIndentLine($"AddObjectInstance(instanceId, new global::{realObjectInvokerTypeFullName}(instance, instanceId));");
                    _sourceBuilder.AppendIndentLine("return null;");
                }, new VariableName(vars)
                {
                    Executor = "this",
                });
            });
            _sourceBuilder.Dedent();
        }
    }

    #endregion ObjectConstructor

    private void GenerateAllStaticMethodProcessCode()
    {
        var staticMethods = Context.Resources.GetAllMethods().Where(m => m.IsStatic).ToArray();
        foreach (var method in staticMethods)
        {
            SourceCodeGenerateHelper.GenerateMethodInvokeThroughMessageCaseScopeCode(Context, Context.Resources, _sourceBuilder, method, new VariableName(_vars) { RunningToken = "__cancellation__" });
        }
    }

    #endregion Generate

    #endregion Private 方法

    #region Public 方法

    public IEnumerable<SourceCode> GetSources()
    {
        yield return new FullSourceCode(SourceHintName, GeneratePartialContextExecutorSource());
    }

    #endregion Public 方法
}
