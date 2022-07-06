using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate;

public class JuxtaposeContextCodeGenerator : ISourceCodeProvider<SourceCode>
{
    #region Private 字段

    private readonly ClassStringBuilder _sourceBuilder = new();

    private string? _generatedSource = null;

    #endregion Private 字段

    #region Public 属性

    public JuxtaposeSourceGeneratorContext Context { get; }

    /// <summary>
    /// 上下文类型符号
    /// </summary>
    public INamedTypeSymbol ContextTypeSymbol { get; }

    public IllusionAttributeDefine[] IllusionAttributeDefines { get; }

    public IllusionAttributeDefine[] IllusionStaticAttributeDefines { get; }

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

    public JuxtaposeContextCodeGenerator(JuxtaposeSourceGeneratorContext context, INamedTypeSymbol contextTypeSymbol)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        ContextTypeSymbol = contextTypeSymbol ?? throw new ArgumentNullException(nameof(contextTypeSymbol));

        TypeFullName = contextTypeSymbol.ToDisplayString();

        Namespace = contextTypeSymbol.ContainingNamespace.ToDisplayString();

        var allIllusionAttributes = contextTypeSymbol.GetAttributes()
                                                     .Where(m => m.IsIllusionAttribute())
                                                     .Select(m => new IllusionAttributeDefine(m))
                                                     .ToArray();

        IllusionAttributeDefines = allIllusionAttributes.Where(m => !m.TargetType.IsStatic).ToArray();
        IllusionStaticAttributeDefines = allIllusionAttributes.Where(m => m.TargetType.IsStatic).ToArray();

        SourceHintName = $"{TypeFullName}.Context.g.cs";
    }

    #endregion Public 构造函数

    #region Private 方法

    private void AppendMessageMapItem(string messageType)
    {
        _sourceBuilder.AppendIndentLine($"typeof({messageType}),");
    }

    private string GeneratePartialContextSource(string hash)
    {
        if (_generatedSource != null)
        {
            return _generatedSource;
        }

        _sourceBuilder.AppendLine(Constants.JuxtaposeGenerateCodeHeader);
        _sourceBuilder.AppendLine();

        var anyTypeFromIoCContainer = Context.IllusionInstanceClasses.Keys.Any(m => m.FromIoCContainer);

        _sourceBuilder.Namespace(() =>
        {
            _sourceBuilder.AppendIndentLine($"partial class {ContextTypeSymbol.Name} : {TypeFullNames.Juxtapose.JuxtaposeContext}{(anyTypeFromIoCContainer ? ", global::Juxtapose.IIoCContainerProvider" : string.Empty)}");
            _sourceBuilder.Scope(() =>
            {
                //TODO 框架调整时，如果有默认消息添加，则在此添加默认的消息
                var allDefaultMessageTypes = new[] {
                    TypeFullNames.Juxtapose.Messages.JuxtaposeAckMessage,
                    TypeFullNames.Juxtapose.Messages.ExceptionMessage,
                    $"{TypeFullNames.Juxtapose.Messages.InstanceMethodInvokeMessage}<global::Juxtapose.Messages.ParameterPacks.CancellationTokenSourceCancelParameterPack>",
                    TypeFullNames.Juxtapose.Messages.DisposeObjectInstanceMessage,
                    $"{TypeFullNames.Juxtapose.Messages.CreateObjectInstanceMessage}<global::Juxtapose.Messages.ParameterPacks.ServiceProviderGetInstanceParameterPack>",
                };

                var allConstructorMessageTypes = Context.Resources.GetAllConstructorParameterPacks().Select(GetParameterPackMessageTypeName).OrderBy(m => m).ToArray();

                var allDelegateInvokeMessageTypes = Context.Resources.GetAllDelegateParameterPacks().Select(GetParameterPackMessageTypeName).OrderBy(m => m).ToArray();
                var allDelegateResultMessageTypes = Context.Resources.GetAllDelegateResultPacks().Select(GetResultPackMessageTypeName).OrderBy(m => m).ToArray();

                var allMethodInvokeMessageTypes = Context.Resources.GetAllMethodParameterPacks().Select(GetParameterPackMessageTypeName).OrderBy(m => m).ToArray();
                var allMethodResultMessageTypes = Context.Resources.GetAllMethodResultPacks().Select(GetResultPackMessageTypeName).OrderBy(m => m).ToArray();

                var allMessageTypes = allDefaultMessageTypes.Concat(allConstructorMessageTypes)
                                                            .Concat(allDelegateInvokeMessageTypes)
                                                            .Concat(allDelegateResultMessageTypes)
                                                            .Concat(allMethodInvokeMessageTypes)
                                                            .Concat(allMethodResultMessageTypes);

                var newHashString = hash + string.Join(", ", allMessageTypes);

                //_sourceBuilder.AppendIndentLine($"// {newHashString}");

                hash = newHashString.CalculateMd5();

                var contextIdentifier = $"{TypeFullName}_{hash}";

                _sourceBuilder.AppendIndentLine($"private static readonly global::System.Lazy<{ContextTypeSymbol.Name}> s_lazySharedInstance = new global::System.Lazy<{ContextTypeSymbol.Name}>(() => new {ContextTypeSymbol.Name}(), global::System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);", true);

                _sourceBuilder.AppendIndentLine($"public static {ContextTypeSymbol.Name} SharedInstance => s_lazySharedInstance.Value;", true);

                _sourceBuilder.AppendIndentLine($"public override global::Juxtapose.CommunicationOptions CommunicationOptions => new(global::Juxtapose.Communication.Channel.NamedPipeCommunicationChannelFactory.Shared, global::Juxtapose.LengthBasedFrameCodecFactory.Shared, new {TypeFullNames.Juxtapose.ConstantMessageCodecFactory}(MessageTypes, LoggerFactory));", true);

                _sourceBuilder.AppendIndentLine($"public override string Identifier => \"{contextIdentifier}\";", true);

                _sourceBuilder.AppendIndentLine($"public override global::Juxtapose.IReadOnlySettingCollection Options {{ get; }} = new global::Juxtapose.JuxtaposeOptions()");
                _sourceBuilder.Scope(() =>
                {
                    _sourceBuilder.AppendIndentLine($"ContextIdentifier = \"{contextIdentifier}\",");
                    _sourceBuilder.AppendIndentLine($"Version = {Juxtapose.Constants.Version},");
                });
                _sourceBuilder.AppendIndentLine(";", true);

                _sourceBuilder.AppendIndentLine($"public override int Version => {Juxtapose.Constants.Version};", true);

                _sourceBuilder.AppendIndentLine($"public override {TypeFullNames.Juxtapose.JuxtaposeExecutor} CreateExecutor({TypeFullNames.Juxtapose.IMessageExchanger} messageExchanger)");
                _sourceBuilder.Scope(() =>
                {
                    _sourceBuilder.AppendIndentLine("ThrowIfDisposed();");
                    _sourceBuilder.AppendIndentLine($"return new {ContextTypeSymbol.Name}.InternalJuxtaposeExecutor(messageExchanger, LoggerFactory.CreateLogger(\"{ContextTypeSymbol.ToDisplayString()}.InternalJuxtaposeExecutor\"){(anyTypeFromIoCContainer ? ", GetIoCContainerAsync" : string.Empty)});");
                });

                _sourceBuilder.AppendLine();

                _sourceBuilder.AppendIndentLine($"public static global::System.Collections.Generic.IReadOnlyList<global::System.Type> MessageTypes {{ get; }} = new global::System.Collections.Generic.List<global::System.Type>()");

                _sourceBuilder.Scope(() =>
                {
                    foreach (var type in allMessageTypes)
                    {
                        AppendMessageMapItem(type);
                    }
                });
                _sourceBuilder.AppendIndentLine(".AsReadOnly();");
            });
        }, Namespace);

        _generatedSource = _sourceBuilder.ToString();

        return _generatedSource;

        static string GetParameterPackMessageTypeName(ParameterPackSourceCode invokeMessageType)
        {
            return invokeMessageType.MethodSymbol.IsStatic
                    ? $"{TypeFullNames.Juxtapose.Messages.StaticMethodInvokeMessage}<{invokeMessageType.TypeName}>"
                    : invokeMessageType.MethodSymbol.MethodKind != MethodKind.Constructor
                        ? $"{TypeFullNames.Juxtapose.Messages.InstanceMethodInvokeMessage}<{invokeMessageType.TypeName}>"
                        : $"{TypeFullNames.Juxtapose.Messages.CreateObjectInstanceMessage}<{invokeMessageType.TypeName}>";
        }

        static string GetResultPackMessageTypeName(ResultPackSourceCode resultMessageType)
        {
            return resultMessageType.MethodSymbol.IsStatic
                    ? $"{TypeFullNames.Juxtapose.Messages.StaticMethodInvokeResultMessage}<{resultMessageType.TypeName}>"
                    : $"{TypeFullNames.Juxtapose.Messages.InstanceMethodInvokeResultMessage}<{resultMessageType.TypeName}>";
        }
    }

    #endregion Private 方法

    #region Public 方法

    public IEnumerable<SourceCode> GetSources()
    {
        Context.Clear();

        var contextHashBuilder = new ContextHashBuilder();

        foreach (var illusionAttributeDefine in IllusionAttributeDefines)
        {
            var descriptor = new IllusionInstanceClassDescriptor(illusionAttributeDefine, ContextTypeSymbol);
            var resources = new SubResourceCollection(Context.Resources);

            if (Context.IllusionInstanceClasses.ContainsKey(descriptor))
            {
                Context.GeneratorExecutionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MultipleIllusionClassDefine, null, descriptor.TargetType, descriptor.InheritType));
                continue;
            }

            Context.IllusionInstanceClasses.Add(descriptor, resources);

            contextHashBuilder.AddIllusionInstanceClassDescriptor(descriptor);

            var illusionClassCodeGenerator = new IllusionClassCodeGenerator(Context, descriptor, resources);

            foreach (var sourceInfo in illusionClassCodeGenerator.GetSources())
            {
                resources.AddSourceCode(sourceInfo);
                yield return sourceInfo;
            }

            var realObjectInvokerCodeGenerator = new RealObjectInvokerCodeGenerator(Context, descriptor, resources);
            foreach (var sourceInfo in realObjectInvokerCodeGenerator.GetSources())
            {
                resources.AddSourceCode(sourceInfo);
                yield return sourceInfo;
            }
        }

        foreach (var illusionStaticAttributeDefine in IllusionStaticAttributeDefines)
        {
            if (illusionStaticAttributeDefine.FromIoCContainer)
            {
                Context.GeneratorExecutionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.StaticTypeCanNotProvidedByServiceProvider, null, illusionStaticAttributeDefine.TargetType.ToDisplayString()));
            }

            var descriptor = new IllusionStaticClassDescriptor(illusionStaticAttributeDefine, ContextTypeSymbol);
            var resources = new SubResourceCollection(Context.Resources);

            if (Context.IllusionStaticClasses.ContainsKey(descriptor))
            {
                Context.GeneratorExecutionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MultipleIllusionStaticClassDefine, null, descriptor.TargetType));
                continue;
            }

            Context.IllusionStaticClasses.Add(descriptor, resources);

            var illusionStaticClassCodeGenerator = new IllusionStaticClassCodeGenerator(Context, descriptor, resources);

            contextHashBuilder.AddIllusionStaticClassCodeGenerator(descriptor);

            foreach (var sourceInfo in illusionStaticClassCodeGenerator.GetSources())
            {
                yield return sourceInfo;
            }
        }

        var executorCodeGenerator = new JuxtaposeExecutorCodeGenerator(Context, ContextTypeSymbol);

        foreach (var item in executorCodeGenerator.GetSources())
        {
            yield return item;
        }

        var hash = contextHashBuilder.GetHash();

        yield return new FullSourceCode(SourceHintName, GeneratePartialContextSource(hash));
    }

    #endregion Public 方法

    #region Private 类

    private class ContextHashBuilder
    {
        #region Private 字段

        private readonly StringBuilder _builder = new();

        #endregion Private 字段

        #region Private 方法

        private void AppendProxyableMembers(INamedTypeSymbol typeSymbol, bool withConstructor)
        {
            foreach (var item in typeSymbol.GetProxyableMembers(withConstructor))
            {
                _builder.Append(item.ToFullyQualifiedDisplayString());
                _builder.Append('&');
            }
        }

        #endregion Private 方法

        #region Public 方法

        public void AddIllusionInstanceClassDescriptor(IllusionInstanceClassDescriptor descriptor)
        {
            _builder.Append(descriptor.TypeFullName);
            _builder.Append('@');
            _builder.Append(descriptor.Accessibility);
            _builder.Append('@');
            _builder.Append(descriptor.FromIoCContainer);
            _builder.Append('@');

            if (descriptor.InheritType is not null)
            {
                AppendProxyableMembers(descriptor.InheritType, false);
            }

            AppendProxyableMembers(descriptor.TargetType, true);
        }

        public void AddIllusionStaticClassCodeGenerator(IllusionStaticClassDescriptor descriptor)
        {
            _builder.Append(descriptor.TypeFullName);
            _builder.Append('@');
            _builder.Append(descriptor.Accessibility);
            _builder.Append('@');

            AppendProxyableMembers(descriptor.TargetType, false);
        }

        public string GetHash()
        {
            return _builder.ToString().CalculateMd5();
        }

        #endregion Public 方法
    }

    #endregion Private 类
}