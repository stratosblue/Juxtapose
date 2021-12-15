using System;
using System.Collections.Generic;

using Juxtapose.SourceGenerator.Internal;
using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate
{
    public class RealObjectInvokerCodeGenerator : ISourceCodeProvider<SourceCode>
    {
        #region Private 字段

        private readonly ClassStringBuilder _sourceBuilder = new();

        private readonly VariableName _vars;

        private string? _generatedSource = null;

        #endregion Private 字段

        #region Public 属性

        public JuxtaposeSourceGeneratorContext Context { get; }

        public INamedTypeSymbol ImplementTypeSymbol { get; }

        public INamedTypeSymbol? InheritTypeSymbol { get; }

        public string Namespace { get; }

        public string SourceHintName { get; }

        public string TypeFullName { get; }

        public string TypeName { get; }

        #endregion Public 属性

        #region Public 构造函数

        public RealObjectInvokerCodeGenerator(JuxtaposeSourceGeneratorContext context,
                                              INamedTypeSymbol implementTypeSymbol,
                                              INamedTypeSymbol? inheritTypeSymbol,
                                              string @namespace,
                                              string targetTypeName)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));

            ImplementTypeSymbol = implementTypeSymbol ?? throw new ArgumentNullException(nameof(implementTypeSymbol));
            InheritTypeSymbol = inheritTypeSymbol;

            Namespace = @namespace;

            TypeFullName = $"{@namespace}.{targetTypeName}RealObjectInvoker";
            TypeName = TypeFullName.Substring(Namespace.Length + 1);

            SourceHintName = $"{TypeFullName}.RealObjectInvoker.g.cs";

            _vars = new VariableName() { Executor = "executor" };
        }

        #endregion Public 构造函数

        #region Private 方法

        private void GenerateProxyClassSource()
        {
            if (!Context.TargetGenerateTypeMethods.TryGetValue(InheritTypeSymbol ?? ImplementTypeSymbol, out var methods))
            {
                throw new ArgumentException($"can not find {InheritTypeSymbol ?? ImplementTypeSymbol} methods in Context.");
            }

            _sourceBuilder.AppendLine(Constants.JuxtaposeGenerateCodeHeader);
            _sourceBuilder.AppendLine();

            _sourceBuilder.Namespace(() =>
            {
                _sourceBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
                _sourceBuilder.AppendIndentLine($"internal partial class {TypeName} : {TypeFullNames.Juxtapose.IMessageExecutor}, {TypeFullNames.System.IDisposable}");
                _sourceBuilder.Scope(() =>
                {
                    _sourceBuilder.AppendLine(@"private readonly int _instanceId;
private CancellationTokenSource _runningTokenSource;
private readonly CancellationToken _runningToken;");
                    _sourceBuilder.AppendIndentLine($"private {ImplementTypeSymbol.ToFullyQualifiedDisplayString()} _instance;", true);

                    _sourceBuilder.AppendLine($@"public {TypeName}({ImplementTypeSymbol.ToFullyQualifiedDisplayString()} instance, int instanceId)
{{
    _instance = instance ?? throw new global::System.ArgumentNullException(nameof(instance));
    _instanceId = instanceId;
    _runningTokenSource = new CancellationTokenSource();
    _runningToken = _runningTokenSource.Token;
}}");

                    _sourceBuilder.AppendLine();

                    _sourceBuilder.AppendIndentLine($"async global::System.Threading.Tasks.Task<{TypeFullNames.Juxtapose.Messages.JuxtaposeMessage}?> {TypeFullNames.Juxtapose.IMessageExecutor}.ExecuteAsync({TypeFullNames.Juxtapose.JuxtaposeExecutor} executor, {TypeFullNames.Juxtapose.Messages.JuxtaposeMessage} message)");

                    _sourceBuilder.Scope(() =>
                    {
                        _sourceBuilder.AppendIndentLine("ThrowIfDisposed();", true);
                        _sourceBuilder.AppendIndentLine("switch (message)");
                        _sourceBuilder.Scope(() =>
                        {
                            foreach (var method in methods)
                            {
                                SourceCodeGenerateHelper.GenerateMethodInvokeThroughMessageCaseScopeCode(Context, _sourceBuilder, method, _vars);
                            }

                            _sourceBuilder.AppendIndentLine($"default: throw new global::System.InvalidOperationException($\"can not process with {{ message }}\");");
                        });
                    });

                    _sourceBuilder.AppendLine($@"private bool _isDisposed = false;

private void ThrowIfDisposed()
{{
    if (_isDisposed)
    {{
        throw new ObjectDisposedException(""{TypeName}"");
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
    _runningTokenSource.Cancel();
    _runningTokenSource.Dispose();
    _runningTokenSource = null!;

    if(_instance is {TypeFullNames.System.IDisposable} disposable)
    {{
        disposable.Dispose();
    }}
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
            var sourceCode = new RealObjectInvokerSourceCode(SourceHintName, GenerateProxyTypeSource(), TypeName, TypeFullName);

            Context.TryAddRealObjectInvokerSourceCode(ImplementTypeSymbol, InheritTypeSymbol, sourceCode);

            yield return sourceCode;
        }

        #endregion Public 方法
    }
}