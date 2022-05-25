using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Juxtapose.SourceGenerator.Internal;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model
{
    public class MethodParamPackContext
    {
        #region Private 字段

        private string[]? _parameterNames;
        private string? _paramPackClassCode;
        private string? _paramPackClassName;
        private string? _resultPackClassCode;
        private string? _resultPackClassName;

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 参数中的<see cref="CancellationToken"/>
        /// </summary>
        public ImmutableArray<IParameterSymbol> CancellationTokenParams { get; }

        /// <summary>
        /// 参数中的委托
        /// </summary>
        public ImmutableArray<IParameterSymbol> DelegateParams { get; }

        /// <summary>
        /// 对应的方法符号
        /// </summary>
        public IMethodSymbol MethodSymbol { get; }

        /// <summary>
        /// 普通参数
        /// </summary>
        public ImmutableArray<IParameterSymbol> NormalParams { get; }

        /// <summary>
        /// 参数名称列表
        /// </summary>
        public string[] ParameterNames => _parameterNames ??= MethodSymbol.Parameters.Select(m => m.Name).ToArray();

        /// <summary>
        /// 参数包的代码
        /// </summary>
        public string ParamPackClassCode => _paramPackClassCode ??= GenParamPackClassCode();

        /// <summary>
        /// 参数包的类名
        /// </summary>
        public string ParamPackClassName => _paramPackClassName ??= GenParamPackClassName();

        /// <summary>
        /// 结果包的代码
        /// </summary>
        public string? ResultPackClassCode => _resultPackClassCode ??= GenResultPackClassCode();

        /// <summary>
        /// 结果包的类名
        /// </summary>
        public string? ResultPackClassName => _resultPackClassName ??= MethodSymbol.GetReturnType() is null ? null : MethodSymbol.GetResultPackClassName();

        #endregion Public 属性

        #region Public 构造函数

        public MethodParamPackContext(IMethodSymbol methodSymbol)
        {
            MethodSymbol = methodSymbol ?? throw new ArgumentNullException(nameof(methodSymbol));

            CancellationTokenParams = MethodSymbol.Parameters.Where(m => m.Type.IsCancellationToken()).ToImmutableArray();

            DelegateParams = MethodSymbol.Parameters.Where(m => m.Type.IsDelegate()).ToImmutableArray();

            //HACK 支持其它特殊参数时需要处理
            NormalParams = MethodSymbol.Parameters.Where(m => !m.Type.IsCancellationToken() && !m.Type.IsDelegate()).ToImmutableArray();
        }

        #endregion Public 构造函数

        #region Private 方法

        private string GenParamPackClassCode()
        {
            var builder = new ClassStringBuilder();

            builder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
            builder.AppendLine($"internal class {ParamPackClassName}");
            builder.Scope(() =>
            {
                for (int i = 0; i < MethodSymbol.Parameters.Length; i++)
                {
                    var parameter = MethodSymbol.Parameters[i];

                    string type;
                    string name;

                    if (parameter.Type.IsCancellationToken()
                        || parameter.Type.IsDelegate())
                    {
                        type = "int?";
                        name = $"{parameter.Name}_RID";
                    }
                    else
                    {
                        type = parameter.Type.ToDisplayString();
                        name = parameter.Name;
                    }

                    builder.AppendIndentLine($"public {type} {name} {{ get; set; }}");

                    if (i < MethodSymbol.Parameters.Length - 1)
                    {
                        builder.AppendLine();
                    }
                }
            });
            return builder.ToString();
        }

        private string? GenResultPackClassCode()
        {
            var returnType = MethodSymbol.GetReturnType();
            if (returnType is null)
            {
                return null;
            }

            var builder = new ClassStringBuilder();
            builder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]");
            builder.AppendLine($"internal class {ResultPackClassName}");
            builder.Scope(() =>
            {
                var returnTypeName = returnType.ToDisplayString();
                builder.AppendLine($"public {returnTypeName} Result {{ get; set; }}");
                builder.AppendLine();
                builder.AppendIndentLine($"public {ResultPackClassName}({returnTypeName} result) {{ Result = result; }}");
            });
            return builder.ToString();
        }

        /// <summary>
        /// 生成打包参数的代码
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="variableName"></param>
        public void GenParamPackCode(ClassStringBuilder builder, string variableName)
        {
            if (MethodSymbol.Parameters.IsEmpty)
            {
                builder.AppendIndentLine($"var {variableName} = new {ParamPackClassName}();");
                return;
            }

            builder.AppendIndentLine($"var {variableName} = new {ParamPackClassName}()");
            builder.AppendIndentLine("{");
            builder.Indent();

            foreach (var parameter in MethodSymbol.Parameters)
            {
                if (parameter.Type.IsCancellationToken()
                    || parameter.Type.IsDelegate())
                {
                    builder.AppendIndentLine($"{parameter.Name}_RID = {parameter.Name}_RID,");
                }
                else
                {
                    builder.AppendIndentLine($"{parameter.Name} = {parameter.Name},");
                }
            }

            builder.Dedent();
            builder.AppendIndent("};");
            builder.AppendLine();
        }

        /// <summary>
        /// 解包参数的代码
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="scopeBuildAction"></param>
        /// <param name="vars"></param>
        /// <returns></returns>
        public void GenParamUnPackCode(JuxtaposeSourceGeneratorContext context, ClassStringBuilder builder, Action scopeBuildAction, VariableName vars)
        {
            if (MethodSymbol.Parameters.IsEmpty)
            {
                scopeBuildAction();
                builder.AppendLine();
                return;
            }
            const string LocalParamPackVariableName = "___paramPack__";
            builder.AppendIndentLine($"var {LocalParamPackVariableName} = {vars.ParameterPack};");

            foreach (var parameter in CancellationTokenParams)
            {
                builder.AppendIndentLine($"global::Juxtapose.Utils.CancellationTokenInvokeUtil.TryRebuildCancellationTokenSource({LocalParamPackVariableName}.{parameter.Name}_RID, {vars.Executor}, out var {parameter.Name}_CTS, out var {parameter.Name});", true);
            }

            foreach (var parameter in DelegateParams)
            {
                var delegateVars = new VariableName(vars)
                {
                    InstanceId = $"{LocalParamPackVariableName}.{parameter.Name}_RID.Value",
                };

                var callbackMethod = ((INamedTypeSymbol)parameter.Type).DelegateInvokeMethod!;
                var callbackParamPackContext = callbackMethod.GetParamPackContext();

                var callbackBodyBuilder = new ClassStringBuilder();
                callbackBodyBuilder.Indent();
                callbackBodyBuilder.Indent();
                SourceCodeGenerateHelper.GenerateInstanceMethodProxyBodyCode(callbackBodyBuilder, context, callbackMethod, delegateVars);

                builder.AppendLine($@"{parameter.Type.ToFullyQualifiedDisplayString()} {parameter.Name} = null!;
if ({LocalParamPackVariableName}.{parameter.Name}_RID.HasValue)
{{
    {parameter.Name} = {(callbackMethod.ReturnType.IsAwaitable() ? "async " : string.Empty)}({callbackMethod.GenerateMethodArgumentStringWithoutType()}) =>
    {{
{callbackBodyBuilder.ToString().Trim('\r', '\n')}
    }};
}}");
            }

            foreach (var item in NormalParams)
            {
                builder.AppendLine($"var {item.Name} = {LocalParamPackVariableName}.{item.Name};");
            }

            if (CancellationTokenParams.Length > 0)
            {
                builder.AppendIndentLine("try");
                builder.AppendIndentLine("{");
                builder.Indent();
            }

            scopeBuildAction();

            if (CancellationTokenParams.Length > 0)
            {
                builder.Dedent();
                builder.AppendIndentLine("}");
                builder.AppendIndentLine("finally");
                builder.Scope(() =>
                {
                    foreach (var parameter in CancellationTokenParams)
                    {
                        builder.AppendLine($@"if ({LocalParamPackVariableName}.{parameter.Name}_RID.HasValue)
{{
    {vars.Executor}.RemoveObjectInstance({LocalParamPackVariableName}.{parameter.Name}_RID.Value);
    {parameter.Name}_CTS!.Dispose();
}}");
                    }
                });
            }
        }

        #endregion Private 方法

        #region Protected 方法

        /// <summary>
        /// 生成参数包类名
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual string GenParamPackClassName()
        {
            return MethodSymbol.GetParamPackClassName();
        }

        #endregion Protected 方法
    }
}