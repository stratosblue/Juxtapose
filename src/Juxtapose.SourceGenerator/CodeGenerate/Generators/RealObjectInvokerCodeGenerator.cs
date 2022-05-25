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

        public IllusionInstanceClassDescriptor Descriptor { get; }

        public SubResourceCollection Resources { get; }

        public string SourceHintName { get; }

        public string TypeFullName { get; }

        public string TypeName { get; }

        #endregion Public 属性

        #region Public 构造函数

        public RealObjectInvokerCodeGenerator(JuxtaposeSourceGeneratorContext context, IllusionInstanceClassDescriptor descriptor, SubResourceCollection resources)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            Resources = resources ?? throw new ArgumentNullException(nameof(resources));

            TypeFullName = $"{descriptor.TypeFullName}RealObjectInvoker";
            TypeName = TypeFullName.Substring(descriptor.Namespace.Length + 1);

            SourceHintName = $"{TypeFullName}.RealObjectInvoker.g.cs";

            _vars = new VariableName() { Executor = "executor" };
        }

        #endregion Public 构造函数

        #region Private 方法

        private void GenerateProxyClassSource()
        {
            var inheritType = Descriptor.InheritType;
            var targetType = Descriptor.TargetType;

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
                    _sourceBuilder.AppendIndentLine($"private {targetType.ToFullyQualifiedDisplayString()} _instance;", true);

                    _sourceBuilder.AppendLine($@"public {TypeName}({targetType.ToFullyQualifiedDisplayString()} instance, int instanceId)
{{
    _instance = instance ?? throw new global::System.ArgumentNullException(nameof(instance));
    _instanceId = instanceId;
    _runningTokenSource = new CancellationTokenSource();
    _runningToken = _runningTokenSource.Token;
}}");

                    _sourceBuilder.AppendLine();

                    _sourceBuilder.AppendIndentLine($"async global::System.Threading.Tasks.Task<{TypeFullNames.Juxtapose.Messages.JuxtaposeMessage}?> {TypeFullNames.Juxtapose.IMessageExecutor}.ExecuteAsync({TypeFullNames.Juxtapose.JuxtaposeExecutor} {_vars.Executor}, {TypeFullNames.Juxtapose.Messages.JuxtaposeMessage} {_vars.Message})");

                    _sourceBuilder.Scope(() =>
                    {
                        _sourceBuilder.AppendIndentLine("ThrowIfDisposed();", true);
                        _sourceBuilder.AppendIndentLine($"switch ({_vars.Message})");
                        _sourceBuilder.Scope(() =>
                        {
                            foreach (var method in Resources.GetAllMethods())
                            {
                                SourceCodeGenerateHelper.GenerateMethodInvokeThroughMessageCaseScopeCode(Context, _sourceBuilder, method, _vars);
                            }

                            _sourceBuilder.AppendIndentLine($"default: throw new global::System.InvalidOperationException($\"can not process with {{ {_vars.Message} }}\");");
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
            var sourceCode = new RealObjectInvokerSourceCode(SourceHintName, GenerateProxyTypeSource(), TypeName, TypeFullName);

            Resources.TryAddRealObjectInvokerSourceCode(Descriptor.TargetType, Descriptor.InheritType, sourceCode);

            yield return sourceCode;
        }

        #endregion Public 方法
    }
}