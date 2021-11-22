using System;
using System.Collections.Generic;

using Juxtapose.SourceGenerator.Internal;
using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate
{
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
                        _sourceBuilder.AppendIndentLine($"public InternalJuxtaposeExecutor({TypeFullNames.Juxtapose.IMessageExchanger} messageExchanger, global::Microsoft.Extensions.Logging.ILogger logger) : base(messageExchanger, logger) {{ }}", true);

                        _sourceBuilder.AppendIndentLine($"protected override async {TypeFullNames.System.Threading.Tasks.Task}<{TypeFullNames.Juxtapose.Messages.JuxtaposeMessage}?> OnMessageAsync({TypeFullNames.Juxtapose.Messages.JuxtaposeMessage} message, {TypeFullNames.System.Threading.CancellationToken} __cancellation__)");
                        _sourceBuilder.Scope(() =>
                        {
                            _sourceBuilder.AppendIndentLine("switch (message)");
                            _sourceBuilder.Scope(() =>
                            {
                                GenerateAllObjectConstructorProcessCode();

                                GenerateAllStaticMethodProcessCode();

                                _sourceBuilder.AppendLine();
                                _sourceBuilder.AppendIndentLine("default:");
                                _sourceBuilder.Scope(() =>
                                {
                                    _sourceBuilder.AppendIndentLine("return await base.OnMessageAsync(message, __cancellation__);");
                                });
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
            foreach (var constructorMethodInfo in Context.ConstructorMethods)
            {
                GenerateConstructorProcessCode(constructorMethodInfo);
            }
        }

        private void GenerateConstructorProcessCode(KeyValuePair<INamedTypeSymbol, HashSet<IMethodSymbol>> constructorMethodInfo)
        {
            var implementTypeSymbol = constructorMethodInfo.Key;

            var vars = new VariableName(_vars)
            {
                ParameterPack = "typedMessage.ParameterPack!",
            };

            foreach (var constructorMethod in constructorMethodInfo.Value)
            {
                var interfaces = Context.ImplementInterfaces[implementTypeSymbol];

                foreach (var @interface in interfaces)
                {
                    var realObjectInvokerSourceCode = Context.TypeRealObjectInvokers[implementTypeSymbol][@interface];
                    var realObjectInvokerTypeFullName = realObjectInvokerSourceCode.TypeFullName;

                    var parameterPackSourceCode = Context.MethodParameterPacks[constructorMethod];
                    var paramPackContext = constructorMethod.GetParamPackContext();

                    var methodInvokeMessageTypeName = $"{TypeFullNames.Juxtapose.Messages.CreateObjectInstanceMessage}<{parameterPackSourceCode.TypeName}>";

                    _sourceBuilder.AppendIndentLine($"case {methodInvokeMessageTypeName}:");

                    _sourceBuilder.Scope(() =>
                    {
                        _sourceBuilder.AppendIndentLine($"var typedMessage = ({methodInvokeMessageTypeName})message;");
                        _sourceBuilder.AppendIndentLine($"var instanceId = typedMessage.InstanceId;");

                        paramPackContext.GenParamUnPackCode(Context, _sourceBuilder, () =>
                        {
                            _sourceBuilder.AppendIndentLine($"var instance = new global::{constructorMethod.ContainingType.ToDisplayString()}({constructorMethod.GenerateMethodArgumentStringWithoutType()});");

                            _sourceBuilder.AppendIndentLine($"AddObjectInstance(instanceId, new global::{realObjectInvokerTypeFullName}(instance, instanceId));");
                            _sourceBuilder.AppendIndentLine("return null;");
                        }, new VariableName(vars)
                        {
                            Executor = "this",
                        });
                    });
                }
            }
        }

        #endregion ObjectConstructor

        private void GenerateAllStaticMethodProcessCode()
        {
            foreach (var staticTypeMethods in Context.StaticMethods)
            {
                foreach (var method in staticTypeMethods.Value)
                {
                    SourceCodeGenerateHelper.GenerateMethodInvokeThroughMessageCaseScopeCode(Context, _sourceBuilder, method, new VariableName(_vars) { RunningToken = "__cancellation__" });
                }
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
}