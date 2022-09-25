using System;
using System.Collections.Generic;
using System.Linq;

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

    public JuxtaposeSourceGeneratorContext Context { get; }

    /// <summary>
    /// 上下文类型符号
    /// </summary>
    public INamedTypeSymbol ContextTypeSymbol { get; }

    /// <summary>
    /// 上下文的命名空间
    /// </summary>
    public string Namespace { get; }

    public string SourceHintName { get; }

    /// <summary>
    /// 上下文的类型完全名称
    /// </summary>
    public string TypeFullName { get; }

    #endregion Public 属性

    #region Public 构造函数

    public JuxtaposeExecutorCodeGenerator(JuxtaposeSourceGeneratorContext context, INamedTypeSymbol contextTypeSymbol)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        ContextTypeSymbol = contextTypeSymbol ?? throw new ArgumentNullException(nameof(contextTypeSymbol));

        TypeFullName = contextTypeSymbol.ToDisplayString();

        Namespace = contextTypeSymbol.ContainingNamespace.ToDisplayString();

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
                        _sourceBuilder.AppendIndentLine($"switch ({_vars.Message})");
                        _sourceBuilder.Scope(() =>
                        {
                            GenerateAllServiceProviderObjectConstructorProcessCode();

                            GenerateAllObjectConstructorProcessCode();

                            GenerateAllStaticMethodProcessCode();

                            _sourceBuilder.AppendLine();
                            _sourceBuilder.AppendIndentLine("default:");

                            _sourceBuilder.Indent();
                            _sourceBuilder.Scope(() =>
                            {
                                _sourceBuilder.AppendIndentLine($"return await base.OnMessageAsync({_vars.Message}, __cancellation__);");
                            });
                            _sourceBuilder.Dedent();
                        });
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
        foreach (var item in Context.IllusionInstanceClasses)
        {
            GenerateConstructorProcessCode(item.Key, item.Value);
        }
    }

    private void GenerateAllServiceProviderObjectConstructorProcessCode()
    {
        var fromServiceProviderDescriptorMaps = Context.IllusionInstanceClasses.Where(m => m.Key.FromIoCContainer).ToArray();

        if (fromServiceProviderDescriptorMaps.Length == 0)
        {
            return;
        }

        _sourceBuilder.AppendIndentLine("case global::Juxtapose.Messages.CreateObjectInstanceMessage<ServiceProviderGetInstanceParameterPack> createObjectByServiceProviderMessage:");
        _sourceBuilder.Indent();
        _sourceBuilder.AppendIndentLine("{");
        _sourceBuilder.Indent();

        _sourceBuilder.AppendIndentLine("var instanceId = createObjectByServiceProviderMessage.InstanceId;");
        _sourceBuilder.AppendIndentLine("IMessageExecutor realObjectInvoker = createObjectByServiceProviderMessage?.ParameterPack?.TypeFullName switch");
        _sourceBuilder.AppendIndentLine("{");
        _sourceBuilder.Indent();

        foreach (var descriptorMap in fromServiceProviderDescriptorMaps)
        {
            var descriptor = descriptorMap.Key;
            if (!descriptorMap.Value.TryGetRealObjectInvokerSourceCode(descriptor.TargetType, descriptor.InheritType, out var invokerSourceCode)
                || invokerSourceCode is null)
            {
                Context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ExecutorGenerateCanNotFoundGeneratedRealObjectInvoker, null, descriptor.TargetType, descriptor.InheritType));
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

        if (!resources.TryGetRealObjectInvokerSourceCode(descriptor.TargetType, descriptor.InheritType, out var realObjectInvokerSourceCode)
            || realObjectInvokerSourceCode is null)
        {
            Context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ExecutorGenerateCanNotFoundGeneratedRealObjectInvoker, null, descriptor.TargetType, descriptor.InheritType));
            return;
        }

        var realObjectInvokerTypeFullName = realObjectInvokerSourceCode.TypeFullName;

        foreach (var parameterPackSourceCode in resources.GetAllConstructorParameterPacks())
        {
            var paramPackContext = resources.TypeSymbolAnalyzer.GetConstructorParamPackContext(parameterPackSourceCode.MethodSymbol, descriptor.TypeFullName);

            var methodInvokeMessageTypeName = $"{TypeFullNames.Juxtapose.Messages.CreateObjectInstanceMessage}<{parameterPackSourceCode.TypeName}>";

            _sourceBuilder.AppendIndentLine($"case {methodInvokeMessageTypeName}:");

            _sourceBuilder.Indent();
            _sourceBuilder.Scope(() =>
            {
                _sourceBuilder.AppendIndentLine($"var typedMessage = ({methodInvokeMessageTypeName}){_vars.Message};");
                _sourceBuilder.AppendIndentLine($"var instanceId = typedMessage.InstanceId;");

                paramPackContext.GenParamUnPackCode(Context, _sourceBuilder, () =>
                {
                    _sourceBuilder.AppendIndentLine($"var instance = new {parameterPackSourceCode.MethodSymbol.ContainingType.ToFullyQualifiedDisplayString()}({parameterPackSourceCode.MethodSymbol.GenerateMethodArgumentStringWithoutType()});");

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
            SourceCodeGenerateHelper.GenerateMethodInvokeThroughMessageCaseScopeCode(Context, _sourceBuilder, method, new VariableName(_vars) { RunningToken = "__cancellation__" });
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