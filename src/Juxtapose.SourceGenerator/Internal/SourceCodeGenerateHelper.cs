using System;
using System.Linq;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Internal
{
    internal class SourceCodeGenerateHelper
    {
        #region Public 方法

        #region MethodProxyBodyCode

        /// <summary>
        /// 生成实例方法的代理执行代码
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="vars"></param>
        public static void GenerateInstanceMethodProxyBodyCode(ClassStringBuilder builder, JuxtaposeSourceGeneratorContext context, IMethodSymbol method, VariableName vars)
        {
            if (!context.TryGetMethodParameterPackWithDiagnostic(method, out var parameterPackSourceCode))
            {
                return;
            }
            ResultPackSourceCode? resultPackSourceCode = null;
            if (method.GetReturnType() is not null
                && !context.TryGetMethodResultPackWithDiagnostic(method, out resultPackSourceCode))
            {
                return;
            }

            var paramPackContext = method.GetParamPackContext();
            var isAwaitable = method.ReturnType.IsAwaitable();

            builder.AppendLine(vars.MethodBodyPrefixSnippet);

            builder.AppendIndentLine($"{vars.RunningToken}.ThrowIfCancellationRequested();");

            foreach (var parameter in paramPackContext.CancellationTokenParams)
            {
                builder.AppendIndentLine($"{parameter.Name}.ThrowIfCancellationRequested();");
                builder.AppendIndentLine($"int? {parameter.Name}_RID = {parameter.Name}.CanBeCanceled ? {vars.Executor}.InstanceIdGenerator.Next() : null;");
            }

            foreach (var parameter in paramPackContext.DelegateParams)
            {
                var callbackMethod = ((INamedTypeSymbol)parameter.Type).DelegateInvokeMethod!;
                var callbackParamPackContext = callbackMethod.GetParamPackContext();

                var callbackBodyBuilder = new ClassStringBuilder();
                callbackBodyBuilder.Indent();
                callbackBodyBuilder.Indent();
                GenerateMethodInvokeThroughMessageCode(context, callbackBodyBuilder, callbackMethod, new VariableName(vars) { Instance = parameter.Name });

                builder.AppendLine($@"int? {parameter.Name}_RID = null;
if ({parameter.Name} is not null)
{{
    {parameter.Name}_RID = {vars.Executor}.InstanceIdGenerator.Next();
    {vars.Executor}.AddObjectInstance({parameter.Name}_RID.Value, new {(isAwaitable ? "Async" : "Sync")}DelegateMessageExecutor({(isAwaitable ? "async " : string.Empty)}(exector, message) =>
    {{
{callbackBodyBuilder.ToString().Trim('\r', '\n')}
    }}));
}}");
            }

            if (!paramPackContext.CancellationTokenParams.IsEmpty)
            {
                builder.AppendIndentLine("bool shouldSendCancel = false;");
            }

            if (!paramPackContext.CancellationTokenParams.IsEmpty
                || !paramPackContext.DelegateParams.IsEmpty)
            {
                builder.AppendLine(@"try
{");
                builder.Indent();
            }

            paramPackContext.GenParamPackCode(builder, "parameterPack");

            if (!paramPackContext.CancellationTokenParams.IsEmpty)
            {
                builder.AppendIndentLine($"using var localCts = CancellationTokenSource.CreateLinkedTokenSource({vars.RunningToken}, {string.Join(", ", paramPackContext.CancellationTokenParams.Select(m => m.Name))});");
                builder.AppendIndentLine("localCts.Token.ThrowIfCancellationRequested();");
                builder.AppendIndentLine("var localToken = localCts.Token;");
            }
            else
            {
                builder.AppendIndentLine($"var localToken = {vars.RunningToken};");
            }

            if (!paramPackContext.CancellationTokenParams.IsEmpty)
            {
                builder.AppendIndentLine("shouldSendCancel = true;");
            }

            if (resultPackSourceCode is not null)
            {
                builder.AppendIndent("return (");
            }
            else
            {
                builder.AppendIndentSpace();
            }

            if (isAwaitable)
            {
                builder.Append($"await {vars.Executor}.InvokeInstanceMethodMessageAsync");
            }
            else
            {
                builder.Append($"{vars.Executor}.InvokeInstanceMethodMessage");
            }

            if (resultPackSourceCode is not null)
            {
                builder.Append($"<{parameterPackSourceCode.TypeName}, {resultPackSourceCode.TypeName}>");
            }

            builder.Append($"(parameterPack, {vars.InstanceId}, localToken)");

            if (resultPackSourceCode is not null)
            {
                builder.Append(").Result;");
            }
            else
            {
                builder.Append(";");
            }

            builder.AppendLine();

            if (!paramPackContext.CancellationTokenParams.IsEmpty
                || !paramPackContext.DelegateParams.IsEmpty)
            {
                builder.Dedent();

                builder.AppendLine(@"}
finally
{");
                builder.Indent();

                foreach (var parameter in paramPackContext.DelegateParams)
                {
                    builder.AppendLine($@"if ({parameter.Name}_RID.HasValue)
{{
    {vars.Executor}.RemoveObjectInstance({parameter.Name}_RID.Value);
}}");
                }

                foreach (var parameter in paramPackContext.CancellationTokenParams)
                {
                    builder.AppendLine($@"if (shouldSendCancel && {parameter.Name}.IsCancellationRequested && !{vars.RunningToken}.IsCancellationRequested)
{{
    {(isAwaitable ? "await " : string.Empty)}{vars.Executor}.InvokeInstanceMethodMessage{(isAwaitable ? "Async" : string.Empty)}(CancellationTokenSourceCancelParameterPack.Instance, {parameter.Name}_RID!.Value, {vars.RunningToken});
}}");
                }

                builder.Dedent();

                builder.AppendIndentLine("}");
            }
        }

        /// <summary>
        /// 生成静态方法的代理执行代码
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="vars"></param>
        public static void GenerateStaticMethodProxyBodyCode(ClassStringBuilder builder, JuxtaposeSourceGeneratorContext context, IMethodSymbol method, VariableName vars)
        {
            if (!context.TryGetMethodParameterPackWithDiagnostic(method, out var parameterPackSourceCode))
            {
                return;
            }
            ResultPackSourceCode? resultPackSourceCode = null;
            if (method.GetReturnType() is not null
                && !context.TryGetMethodResultPackWithDiagnostic(method, out resultPackSourceCode))
            {
                return;
            }
            var paramPackContext = method.GetParamPackContext();
            var isAwaitable = method.ReturnType.IsAwaitable();

            var awaitTag = isAwaitable ? "await " : string.Empty;
            var asyncTag = isAwaitable ? "Async" : string.Empty;

            vars.Executor = "executorOwner.Executor";

            builder.AppendIndentLine($"IJuxtaposeExecutorOwner? executorOwner = null;");

            foreach (var parameter in paramPackContext.CancellationTokenParams)
            {
                builder.AppendIndentLine($"{parameter.Name}.ThrowIfCancellationRequested();");
                builder.AppendIndentLine($"int? {parameter.Name}_RID = null;");
            }

            foreach (var parameter in paramPackContext.DelegateParams)
            {
                builder.AppendIndentLine($"int? {parameter.Name}_RID = null;");
            }

            if (!paramPackContext.CancellationTokenParams.IsEmpty)
            {
                builder.AppendIndentLine("bool shouldSendCancel = false;");
            }

            builder.AppendLine(@"try
{");
            builder.Indent();

            if (!paramPackContext.CancellationTokenParams.IsEmpty)
            {
                builder.AppendIndentLine($"using var localCts = CancellationTokenSource.CreateLinkedTokenSource({vars.RunningToken}, {string.Join(", ", paramPackContext.CancellationTokenParams.Select(m => m.Name))});");
                builder.AppendIndentLine("localCts.Token.ThrowIfCancellationRequested();");
                builder.AppendIndentLine("var localToken = localCts.Token;");
            }
            else
            {
                builder.AppendIndentLine($"var localToken = CancellationToken.None;");
            }

            builder.AppendIndentLine($"executorOwner = {awaitTag}s_context.GetExecutorOwner{asyncTag}({method.GetCreationContextVariableName()}, localToken);");

            foreach (var parameter in paramPackContext.CancellationTokenParams)
            {
                builder.AppendIndentLine($"{parameter.Name}_RID = {parameter.Name}.CanBeCanceled ? {vars.Executor}.InstanceIdGenerator.Next() : null;");
            }

            foreach (var parameter in paramPackContext.DelegateParams)
            {
                var callbackMethod = ((INamedTypeSymbol)parameter.Type).DelegateInvokeMethod!;
                var callbackParamPackContext = callbackMethod.GetParamPackContext();

                var callbackBodyBuilder = new ClassStringBuilder();
                callbackBodyBuilder.Indent();
                callbackBodyBuilder.Indent();
                GenerateMethodInvokeThroughMessageCode(context, callbackBodyBuilder, callbackMethod, new VariableName(vars)
                {
                    Instance = parameter.Name,
                    InstanceId = $"{parameter.Name}_RID.Value",
                });

                builder.AppendLine($@"if ({parameter.Name} is not null)
{{
    {parameter.Name}_RID = {vars.Executor}.InstanceIdGenerator.Next();
    {vars.Executor}.AddObjectInstance({parameter.Name}_RID.Value, new {(isAwaitable ? "Async" : "Sync")}DelegateMessageExecutor({(isAwaitable ? "async " : string.Empty)}(exector, message) =>
    {{
{callbackBodyBuilder.ToString().Trim('\r', '\n')}
    }}));
}}");
            }

            paramPackContext.GenParamPackCode(builder, "parameterPack");

            if (resultPackSourceCode is not null)
            {
                builder.AppendIndent("return (");
            }
            else
            {
                builder.AppendIndentSpace();
            }

            if (isAwaitable)
            {
                builder.Append($"await {vars.Executor}.InvokeStaticMethodMessageAsync");
            }
            else
            {
                builder.Append($"{vars.Executor}.InvokeStaticMethodMessage");
            }

            if (resultPackSourceCode is not null)
            {
                builder.Append($"<{parameterPackSourceCode.TypeName}, {resultPackSourceCode.TypeName}>");
            }

            builder.Append($"(parameterPack, localToken)");

            if (resultPackSourceCode is not null)
            {
                builder.Append(").Result;");
            }
            else
            {
                builder.Append(";");
            }

            builder.AppendLine();

            builder.Dedent();

            builder.AppendLine(@"}
finally
{");
            builder.Indent();

            builder.AppendIndentLine("if (executorOwner is not null)");
            builder.Scope(() =>
            {
                foreach (var parameter in paramPackContext.DelegateParams)
                {
                    builder.AppendLine($@"if ({parameter.Name}_RID.HasValue)
{{
    {vars.Executor}.RemoveObjectInstance({parameter.Name}_RID.Value);
}}");
                }

                if (!paramPackContext.CancellationTokenParams.IsEmpty)
                {
                    foreach (var parameter in paramPackContext.CancellationTokenParams)
                    {
                        builder.AppendLine($@"if (shouldSendCancel && {parameter.Name}.IsCancellationRequested && !cancellation.IsCancellationRequested)
{{
    {awaitTag}{vars.Executor}.InvokeStaticMethodMessage{asyncTag}(CancellationTokenSourceCancelParameterPack.Instance, {vars.Executor}.RunningToken);
}}");
                    }
                }
                builder.AppendIndentLine("executorOwner.Dispose();");
            });

            builder.Dedent();
            builder.AppendIndentLine("}");
        }

        #endregion MethodProxyBodyCode

        /// <summary>
        /// 创建使用 Juxtapose.Messages 进行方法调用的 case: 代码块
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceBuilder"></param>
        /// <param name="method"></param>
        /// <param name="vars"></param>
        public static void GenerateMethodInvokeThroughMessageCaseScopeCode(JuxtaposeSourceGeneratorContext context, ClassStringBuilder sourceBuilder, IMethodSymbol method, VariableName vars)
        {
            if (!context.TryGetMethodParameterPackWithDiagnostic(method, out var parameterPackSourceCode))
            {
                return;
            }
            var methodInvokeMessageTypeName = GetInvokeMessageFullTypeName(method, parameterPackSourceCode);

            sourceBuilder.AppendIndentLine($"case {methodInvokeMessageTypeName}:");

            sourceBuilder.Indent();

            sourceBuilder.Scope(() =>
            {
                GenerateMethodInvokeThroughMessageCode(context, sourceBuilder, method, vars);
            });

            sourceBuilder.Dedent();
        }

        /// <summary>
        /// 创建使用 Juxtapose.Messages 进行方法调用的代码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceBuilder"></param>
        /// <param name="method"></param>
        /// <param name="vars"></param>
        public static void GenerateMethodInvokeThroughMessageCode(JuxtaposeSourceGeneratorContext context, ClassStringBuilder sourceBuilder, IMethodSymbol method, VariableName vars)
        {
            if (!context.TryGetMethodParameterPackWithDiagnostic(method, out var parameterPackSourceCode))
            {
                return;
            }
            ResultPackSourceCode? resultPackSourceCode = null;
            if (method.GetReturnType() is not null
                && !context.TryGetMethodResultPackWithDiagnostic(method, out resultPackSourceCode))
            {
                return;
            }
            var paramPackContext = method.GetParamPackContext();

            var methodInvokeMessageTypeName = GetInvokeMessageFullTypeName(method, parameterPackSourceCode);
            var methodInvokeResultMessageTypeName = GetInvokeResultMessageFullTypeName(method, resultPackSourceCode);

            sourceBuilder.AppendIndentLine($"var typedMessage = ({methodInvokeMessageTypeName})message;");

            paramPackContext.GenParamUnPackCode(context, sourceBuilder, () =>
            {
                if (method.MethodKind == MethodKind.PropertyGet)
                {
                    var propName = method.Name.Replace("get_", string.Empty).Replace("set_", string.Empty);
                    if (method.IsStatic)
                    {
                        sourceBuilder.AppendIndentLine($"return new {methodInvokeResultMessageTypeName}(message.Id) {{ Result = new({method.ContainingType.ToFullyQualifiedDisplayString()}.{propName}) }};");
                    }
                    else
                    {
                        sourceBuilder.AppendIndentLine($"return new {methodInvokeResultMessageTypeName}(message.Id, {vars.InstanceId}) {{ Result = new({vars.Instance}.{propName}) }};");
                    }
                    return;
                }
                else if (method.MethodKind == MethodKind.PropertySet)
                {
                    var propName = method.Name.Replace("get_", string.Empty).Replace("set_", string.Empty);
                    if (method.IsStatic)
                    {
                        sourceBuilder.AppendIndentLine($"{method.ContainingType.ToFullyQualifiedDisplayString()}.{propName} = value;");
                    }
                    else
                    {
                        sourceBuilder.AppendIndentLine($"{vars.Instance}.{propName} = value;");
                    }
                    sourceBuilder.AppendIndentLine("return null;");
                    return;
                }

                if (method.IsReturnVoidOrTask())
                {
                    sourceBuilder.AppendIndentSpace();
                }
                else
                {
                    sourceBuilder.AppendIndent("var result = ");
                }

                if (method.ReturnType.IsAwaitable())
                {
                    sourceBuilder.Append("await ");
                }

                if (method.IsStatic)
                {
                    sourceBuilder.Append($"{method.ContainingType.ToFullyQualifiedDisplayString()}.{method.Name}({method.GenerateMethodArgumentStringWithoutType()});{Environment.NewLine}");
                }
                else
                {
                    sourceBuilder.Append($"{vars.Instance}.{method.Name}({method.GenerateMethodArgumentStringWithoutType()});{Environment.NewLine}");
                }

                if (method.IsReturnVoidOrTask())
                {
                    sourceBuilder.AppendIndentLine("return null;");
                }
                else
                {
                    if (method.IsStatic)
                    {
                        sourceBuilder.AppendIndentLine($"return new {methodInvokeResultMessageTypeName}(message.Id) {{ Result = new(result) }};");
                    }
                    else
                    {
                        sourceBuilder.AppendIndentLine($"return new {methodInvokeResultMessageTypeName}(message.Id, {vars.InstanceId}) {{ Result = new(result) }};");
                    }
                }
            }, new VariableName(vars)
            {
                ParameterPack = "typedMessage.ParameterPack!",
            });
        }

        #endregion Public 方法

        #region Private 方法

        private static string GetInvokeMessageFullTypeName(IMethodSymbol method, ParameterPackSourceCode parameterPackSourceCode)
        {
            return method.IsStatic
                   ? $"{TypeFullNames.Juxtapose.Messages.StaticMethodInvokeMessage}<{parameterPackSourceCode.TypeName}>"
                   : $"{TypeFullNames.Juxtapose.Messages.InstanceMethodInvokeMessage}<{parameterPackSourceCode.TypeName}>";
        }

        private static string GetInvokeResultMessageFullTypeName(IMethodSymbol method, ResultPackSourceCode? resultPackSourceCode)
        {
            if (resultPackSourceCode == null)
            {
                return string.Empty;
            }
            return method.IsStatic
                   ? $"{TypeFullNames.Juxtapose.Messages.StaticMethodInvokeResultMessage}<{resultPackSourceCode.TypeName}>"
                   : $"{TypeFullNames.Juxtapose.Messages.InstanceMethodInvokeResultMessage}<{resultPackSourceCode.TypeName}>";
        }

        #endregion Private 方法
    }
}