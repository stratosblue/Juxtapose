﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate
{
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

            _sourceBuilder.Namespace(() =>
            {
                _sourceBuilder.AppendIndentLine($"partial class {ContextTypeSymbol.Name} : {TypeFullNames.Juxtapose.JuxtaposeContext}");
                _sourceBuilder.Scope(() =>
                {
                    //TODO 框架调整时，如果有默认消息添加，则在此添加默认的消息
                    var allDefaultMessageTypes = new[] {
                        TypeFullNames.Juxtapose.Messages.JuxtaposeAckMessage,
                        TypeFullNames.Juxtapose.Messages.ExceptionMessage,
                        $"{TypeFullNames.Juxtapose.Messages.InstanceMethodInvokeMessage}<global::Juxtapose.Messages.ParameterPacks.CancellationTokenSourceCancelParameterPack>",
                        TypeFullNames.Juxtapose.Messages.DisposeObjectInstanceMessage,
                    };

                    var allInvokeMessageTypes = Context.GetAllParameterPacks().Select(GetParameterPackMessageTypeName).OrderBy(m => m).ToArray();
                    var allResultMessageTypes = Context.MethodResultPacks.Select(m => m.Value).OfType<ResultPackSourceCode>().Select(GetResultPackMessageTypeName).OrderBy(m => m).ToArray();

                    //if (allInvokeMessageTypes.Length == 0
                    //    || allResultMessageTypes.Length == 0)
                    //{
                    //    throw new ArgumentOutOfRangeException("there is no message packs in context. something wrong.");
                    //}

                    var allMessageTypes = allDefaultMessageTypes.Concat(allInvokeMessageTypes).Concat(allResultMessageTypes);

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
                        _sourceBuilder.AppendIndentLine($"return new {ContextTypeSymbol.Name}.InternalJuxtaposeExecutor(messageExchanger, LoggerFactory.CreateLogger(\"{ContextTypeSymbol.ToDisplayString()}.InternalJuxtaposeExecutor\"));");
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
                var illusionClassCodeGenerator = new IllusionClassCodeGenerator(Context, illusionAttributeDefine, ContextTypeSymbol);

                contextHashBuilder.AddIllusionClass(illusionClassCodeGenerator.ImplementTypeSymbol, illusionClassCodeGenerator.InheritTypeSymbol);

                foreach (var sourceInfo in illusionClassCodeGenerator.GetSources())
                {
                    yield return sourceInfo;
                }

                var realObjectInvokerCodeGenerator = new RealObjectInvokerCodeGenerator(Context, illusionClassCodeGenerator.ImplementTypeSymbol, illusionClassCodeGenerator.InheritTypeSymbol, illusionClassCodeGenerator.Namespace, illusionClassCodeGenerator.TypeName);
                foreach (var sourceInfo in realObjectInvokerCodeGenerator.GetSources())
                {
                    yield return sourceInfo;
                }
            }

            foreach (var illusionStaticAttributeDefine in IllusionStaticAttributeDefines)
            {
                var illusionStaticClassCodeGenerator = new IllusionStaticClassCodeGenerator(Context, illusionStaticAttributeDefine, ContextTypeSymbol);

                contextHashBuilder.AddStaticClass(illusionStaticClassCodeGenerator.StaticClassType);

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

            #region Public 方法

            public void AddIllusionClass(INamedTypeSymbol targetTypeSymbol, INamedTypeSymbol? inheritTypeSymbol)
            {
                if (inheritTypeSymbol is not null)
                {
                    foreach (var item in inheritTypeSymbol.GetProxyableMembers())
                    {
                        _builder.Append(item.ToDisplayString());
                        _builder.Append('&');
                    }
                }

                foreach (var item in targetTypeSymbol.Constructors.Where(m => m.NotStatic()))
                {
                    _builder.Append(item.ToDisplayString());
                    _builder.Append('&');
                }
            }

            public void AddStaticClass(INamedTypeSymbol staticTypeSymbol)
            {
                foreach (var item in staticTypeSymbol.GetProxyableMembers())
                {
                    _builder.Append(item.ToDisplayString());
                    _builder.Append('&');
                }
            }

            public string GetHash()
            {
                return _builder.ToString().CalculateMd5();
            }

            #endregion Public 方法
        }

        #endregion Private 类
    }
}