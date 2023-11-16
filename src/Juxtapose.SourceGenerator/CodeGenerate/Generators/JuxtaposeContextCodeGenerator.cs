using System.Text;
using Juxtapose.SourceGenerator.Internal;
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

    public JuxtaposeContextSourceGeneratorContext Context { get; }

    public JuxtaposeContextDeclaration ContextDeclaration { get; }

    /// <summary>
    /// 上下文类型符号
    /// </summary>
    public INamedTypeSymbol ContextTypeSymbol { get; }

    public IllusionAttributeDefine[] IllusionAttributeDefines { get; }

    public IllusionAttributeDefine[] IllusionStaticAttributeDefines { get; }

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

    public JuxtaposeContextCodeGenerator(JuxtaposeContextSourceGeneratorContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        ContextDeclaration = context.ContextDeclaration;
        ContextTypeSymbol = ContextDeclaration.TypeSymbol ?? throw new ArgumentNullException(nameof(context.ContextDeclaration.TypeSymbol));

        TypeFullName = ContextTypeSymbol.ToDisplayString();

        Namespace = ContextTypeSymbol.ContainingNamespace.GetNamespaceName();

        var allIllusionAttributes = ContextTypeSymbol.GetAttributes()
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

    private string GenerateCommandPartialContextSource()
    {
        var sourceBuilder = new ClassStringBuilder(10240);

        sourceBuilder.AppendLine(Constants.JuxtaposeGenerateCodeHeader);
        sourceBuilder.AppendLine();

        sourceBuilder.Namespace(() =>
        {
            sourceBuilder.AppendIndentLine($"partial class {ContextTypeSymbol.Name}");
            sourceBuilder.Scope(() =>
            {
                sourceBuilder.AppendIndentLine("public enum JuxtaposeGeneratedCommand : int");
                sourceBuilder.AppendIndentLine("{");
                sourceBuilder.Indent();

                var items = new List<(int, string)>();
                foreach (var symbol in Context.Resources.GetAllCommandedSymbol())
                {
                    var commandId = Context.Resources.GetCommandId(symbol);
                    var code = @$"/// <summary>
/// Command for <see cref=""{symbol.ToInheritDocCrefString()}""/>
/// </summary>
Command_{commandId} = {commandId},";

                    items.Add((commandId, code));
                }

                foreach (var item in items.OrderBy(m => m.Item1))
                {
                    sourceBuilder.AppendLine(item.Item2);
                    sourceBuilder.AppendLine();
                }

                sourceBuilder.Dedent();
                sourceBuilder.AppendIndentLine("}");
            });
        }, Namespace);

        return sourceBuilder.ToString();
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
            //TODO 框架调整时，如果有默认消息添加，则在此添加默认的消息
            var allDefaultMessageTypes = new[] {
                    TypeFullNames.Juxtapose.Messages.JuxtaposeAckMessage,
                    TypeFullNames.Juxtapose.Messages.ExceptionMessage,
                    $"{TypeFullNames.Juxtapose.Messages.InstanceMethodInvokeMessage}<global::Juxtapose.Messages.ParameterPacks.CancellationTokenSourceCancelParameterPack>",
                    TypeFullNames.Juxtapose.Messages.DisposeObjectInstanceMessage,
                    $"{TypeFullNames.Juxtapose.Messages.CreateObjectInstanceMessage}<global::Juxtapose.Messages.ParameterPacks.ServiceProviderGetInstanceParameterPack>",
                };

            var allConstructorMessageTypes = Context.Resources.GetAllConstructors().Select(GetParameterPackMessageTypeName).OrderBy(m => m, PersistentStringComparer.Instance).ToArray();

            var allDelegateInvokeMessageTypes = Context.Resources.GetAllDelegates().Select(GetParameterPackMessageTypeName).OrderBy(m => m, PersistentStringComparer.Instance).ToArray();
            var allDelegateResultMessageTypes = Context.Resources.GetAllDelegates().Select(GetResultPackMessageTypeName).OrderBy(m => m, PersistentStringComparer.Instance).ToArray();

            var allMethodInvokeMessageTypes = Context.Resources.GetAllMethods().Select(GetParameterPackMessageTypeName).OrderBy(m => m, PersistentStringComparer.Instance).ToArray();
            var allMethodResultMessageTypes = Context.Resources.GetAllMethods().Select(GetResultPackMessageTypeName).OrderBy(m => m, PersistentStringComparer.Instance).ToArray();

            var allMessageTypes = allConstructorMessageTypes.Concat(allDelegateInvokeMessageTypes)
                                                            .Concat(allDelegateResultMessageTypes)
                                                            .Concat(allMethodInvokeMessageTypes)
                                                            .Concat(allMethodResultMessageTypes)
                                                            .OrderBy(m => m, PersistentStringComparer.Instance)
                                                            .Distinct();
            allMessageTypes = allDefaultMessageTypes.Concat(allMessageTypes).ToList();
            var newHashString = hash + string.Join(", ", allMessageTypes);

            _sourceBuilder.AppendIndentLine($"//.net8+ 使用此代码支持aot");
            foreach (var item in allMessageTypes)
            {
                _sourceBuilder.AppendIndentLine($"//[JsonSerializable(typeof({item}))]");
            }

            _sourceBuilder.AppendIndentLine("//[JsonSourceGenerationOptions(IgnoreReadOnlyProperties = false, IgnoreReadOnlyFields = false, IncludeFields = true, WriteIndented = false)]");
            _sourceBuilder.AppendLine($"//partial class {ContextTypeSymbol.Name}JsonSerializerContext : JsonSerializerContext {{}}");

            _sourceBuilder.AppendLine();

            //_sourceBuilder.AppendIndentLine($"// {newHashString}");

            hash = newHashString.CalculateMd5();

            _sourceBuilder.AppendIndentLine($"partial class {ContextTypeSymbol.Name} : {TypeFullNames.Juxtapose.JuxtaposeContext}{(anyTypeFromIoCContainer ? ", global::Juxtapose.IIoCContainerProvider" : string.Empty)}");
            _sourceBuilder.Scope(() =>
            {
                var contextIdentifier = $"{TypeFullName}_{hash}";

                _sourceBuilder.AppendIndentLine($"private static readonly global::System.Lazy<{ContextTypeSymbol.Name}> s_lazySharedInstance = new global::System.Lazy<{ContextTypeSymbol.Name}>(() => new {ContextTypeSymbol.Name}(), global::System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);", true);

                _sourceBuilder.AppendIndentLine($"public static {ContextTypeSymbol.Name} SharedInstance => s_lazySharedInstance.Value;", true);

                _sourceBuilder.AppendIndentLine($"public sealed override string Identifier => \"{contextIdentifier}\";", true);

                _sourceBuilder.AppendIndentLine($"public sealed override global::Juxtapose.IReadOnlySettingCollection Options {{ get; }} = new global::Juxtapose.JuxtaposeOptions()");
                _sourceBuilder.Scope(() =>
                {
                    _sourceBuilder.AppendIndentLine($"ContextIdentifier = \"{contextIdentifier}\",");
                    _sourceBuilder.AppendIndentLine($"Version = {Juxtapose.Constants.Version},");
                });
                _sourceBuilder.AppendIndentLine(";", true);

                _sourceBuilder.AppendIndentLine($"public sealed override int Version => {Juxtapose.Constants.Version};", true);

                _sourceBuilder.AppendIndentLine($"public override {TypeFullNames.Juxtapose.JuxtaposeExecutor} CreateExecutor({TypeFullNames.Juxtapose.IMessageExchanger} messageExchanger)");
                _sourceBuilder.Scope(() =>
                {
                    _sourceBuilder.AppendIndentLine("ThrowIfDisposed();");
                    _sourceBuilder.AppendIndentLine($"return new {ContextTypeSymbol.Name}.InternalJuxtaposeExecutor(messageExchanger, LoggerFactory.CreateLogger(\"{ContextTypeSymbol.ToDisplayString()}.InternalJuxtaposeExecutor\"){(anyTypeFromIoCContainer ? ", GetIoCContainerAsync" : string.Empty)});");
                });

                _sourceBuilder.AppendLine();

                _sourceBuilder.AppendIndentLine($"protected override global::System.Collections.Generic.IEnumerable<Type> GetMessageTypes() => MessageTypes;", true);

                _sourceBuilder.AppendIndentLine($"public static global::System.Collections.Generic.IReadOnlyList<global::System.Type> MessageTypes {{ get; }} = new global::System.Collections.Generic.List<global::System.Type>()");

                _sourceBuilder.Scope(() =>
                {
                    foreach (var type in allMessageTypes)
                    {
                        AppendMessageMapItem(type);
                    }
                });
                _sourceBuilder.AppendIndentLine(".AsReadOnly();");

                _sourceBuilder.AppendLine();

                _sourceBuilder.AppendIndentLine($"//.net8+ 使用此代码支持aot");
                _sourceBuilder.AppendLine($@"//protected override ICommunicationMessageCodecFactory CreateCommunicationMessageCodecFactory()
//{{
//#if NET8_0_OR_GREATER
//    var communicationMessageCodec = new DefaultJsonBasedMessageCodec(GetMessageTypes(), LoggerFactory, {ContextTypeSymbol.Name}JsonSerializerContext.Default.Options);
//    return new GenericSharedMessageCodecFactory(communicationMessageCodec);
//#else
//    return base.CreateCommunicationMessageCodecFactory();
//#endif
//}}");
            });
        }, Namespace);

        _generatedSource = _sourceBuilder.ToString();

        return _generatedSource;

        string GetParameterPackMessageTypeName(IMethodSymbol methodSymbol)
        {
            var typeName = ArgumentsAndResultsHelper.GenerateValueTupleTypeName(methodSymbol.Parameters.Select(m => m.Type.WithNullableAnnotation(NullableAnnotation.None)), Context.TypeSymbolAnalyzer);
            return methodSymbol.IsStatic
                    ? $"{TypeFullNames.Juxtapose.Messages.StaticMethodInvokeMessage}<{typeName}>"
                    : methodSymbol.MethodKind != MethodKind.Constructor
                        ? $"{TypeFullNames.Juxtapose.Messages.InstanceMethodInvokeMessage}<{typeName}>"
                        : $"{TypeFullNames.Juxtapose.Messages.CreateObjectInstanceMessage}<{typeName}>";
        }

        string GetResultPackMessageTypeName(IMethodSymbol methodSymbol)
        {
            var types = Context.TypeSymbolAnalyzer.GetReturnType(methodSymbol)?.WithNullableAnnotation(NullableAnnotation.None) is ITypeSymbol returnType
                        ? [returnType]
                        : Array.Empty<ITypeSymbol>();
            var typeName = ArgumentsAndResultsHelper.GenerateValueTupleTypeName(types, Context.TypeSymbolAnalyzer);
            return methodSymbol.IsStatic
                    ? $"{TypeFullNames.Juxtapose.Messages.StaticMethodInvokeResultMessage}<{typeName}>"
                    : $"{TypeFullNames.Juxtapose.Messages.InstanceMethodInvokeResultMessage}<{typeName}>";
        }
    }

    #endregion Private 方法

    #region Public 方法

    public IEnumerable<SourceCode> GetSources()
    {
        var contextHashBuilder = new ContextHashBuilder();

        foreach (var illusionInstanceClassesKV in Context.IllusionInstanceClasses)
        {
            var descriptor = illusionInstanceClassesKV.Key;
            var resources = illusionInstanceClassesKV.Value;

            contextHashBuilder.AddIllusionInstanceClassDescriptor(descriptor);

            var illusionClassSources = new IllusionClassCodeGenerator(Context, descriptor, resources).GetSources();
            var realObjectInvokerSources = new RealObjectInvokerCodeGenerator(Context, descriptor, resources).GetSources();

            foreach (var sourceInfo in illusionClassSources.Concat(realObjectInvokerSources))
            {
                resources.AddSourceCode(sourceInfo);
                yield return sourceInfo;
            }
        }

        foreach (var illusionStaticClassesKV in Context.IllusionStaticClasses)
        {
            var descriptor = illusionStaticClassesKV.Key;
            var resources = illusionStaticClassesKV.Value;

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

        var commandPartialContextSource = GenerateCommandPartialContextSource();

        contextHashBuilder.AppendString(commandPartialContextSource);

        var hash = contextHashBuilder.GetHash();

        yield return new FullSourceCode(SourceHintName, GeneratePartialContextSource(hash));
        yield return new FullSourceCode($"{TypeFullName}.Command.g.cs", commandPartialContextSource);
    }

    public JuxtaposeContextCodeGenerator Preparation()
    {
        //准备数据，有序定义所有支持的指令
        var orderedIllusionAttributeDefines = IllusionAttributeDefines.OrderBy(m => m.TargetType.Name, PersistentStringComparer.Instance)
                                                                      .ThenBy(m => m.GeneratedTypeName ?? string.Empty, PersistentStringComparer.Instance)
                                                                      .ThenBy(m => m.FromIoCContainer)
                                                                      .ThenBy(m => m.Accessibility);

        foreach (var illusionAttributeDefine in orderedIllusionAttributeDefines)
        {
            var descriptor = new IllusionInstanceClassDescriptor(illusionAttributeDefine, ContextTypeSymbol);

            if (Context.IllusionInstanceClasses.ContainsKey(descriptor))
            {
                Context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MultipleIllusionClassDefine, null, descriptor.TargetType, null));
                continue;
            }

            var resources = new SubResourceCollection(Context.Resources);
            Context.IllusionInstanceClasses.TryAdd(descriptor, resources);

            foreach (var constructor in descriptor.TargetType.Constructors.Where(m => m.NotStatic()).OrderBy(m => m.ToString(), PersistentStringComparer.Instance))
            {
                resources.AddConstructors(constructor);
            }

            var typeMembers = descriptor.TargetType.GetProxyableMembers(false)
                                                   .OrderBy(m => m.Name, PersistentStringComparer.Instance);

            AppendProxyableMembers(resources, typeMembers);
        }

        var orderedIllusionStaticAttributeDefines = IllusionStaticAttributeDefines.OrderBy(m => m.TargetType.Name, PersistentStringComparer.Instance)
                                                                                  .ThenBy(m => m.GeneratedTypeName ?? string.Empty, PersistentStringComparer.Instance)
                                                                                  .ThenBy(m => m.FromIoCContainer)
                                                                                  .ThenBy(m => m.Accessibility);

        foreach (var illusionStaticAttributeDefine in orderedIllusionStaticAttributeDefines)
        {
            if (illusionStaticAttributeDefine.FromIoCContainer)
            {
                Context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.StaticTypeCanNotProvidedByServiceProvider, null, illusionStaticAttributeDefine.TargetType.ToDisplayString()));
                continue;
            }

            var descriptor = new IllusionStaticClassDescriptor(illusionStaticAttributeDefine, ContextTypeSymbol);

            if (Context.IllusionStaticClasses.ContainsKey(descriptor))
            {
                Context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MultipleIllusionStaticClassDefine, null, descriptor.TargetType));
                continue;
            }

            var resources = new SubResourceCollection(Context.Resources);
            Context.IllusionStaticClasses.TryAdd(descriptor, resources);

            var typeMembers = descriptor.TargetType.GetProxyableMembers(false)
                                                   .Where(m => m.DeclaredAccessibility == Accessibility.Public)
                                                   .OrderBy(m => m.Name, PersistentStringComparer.Instance);

            AppendProxyableMembers(resources, typeMembers);
        }

        return this;

        static void AppendMethod(ResourceCollection resources, IMethodSymbol? methodSymbol)
        {
            if (methodSymbol is null)
            {
                return;
            }
            resources.AddMethods(methodSymbol);

            foreach (var delegateParameter in methodSymbol.Parameters.Where(m => m.Type.IsDelegate()).OrderBy(m => m.Name, PersistentStringComparer.Instance))
            {
                var callbackMethod = ((INamedTypeSymbol)delegateParameter.Type).DelegateInvokeMethod!;
                resources.AddDelegates(callbackMethod);
            }
        }

        static void AppendProxyableMembers(SubResourceCollection resources, IOrderedEnumerable<ISymbol> typeMembers)
        {
            foreach (var typeMember in typeMembers)
            {
                switch (typeMember.Kind)
                {
                    case SymbolKind.Property:
                        var propertySymbol = (IPropertySymbol)typeMember;
                        AppendMethod(resources, propertySymbol.GetMethod);
                        AppendMethod(resources, propertySymbol.SetMethod);
                        resources.AddProperties(propertySymbol);
                        break;

                    case SymbolKind.Method:
                        var methodSymbol = (IMethodSymbol)typeMember;
                        if (methodSymbol.IsPropertyAccessor())
                        {
                            continue;
                        }
                        AppendMethod(resources, methodSymbol);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
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

        public void AppendString(string value)
        {
            _builder.Append(value);
        }

        public string GetHash()
        {
            return _builder.ToString().CalculateMd5();
        }

        #endregion Public 方法
    }

    #endregion Private 类
}
