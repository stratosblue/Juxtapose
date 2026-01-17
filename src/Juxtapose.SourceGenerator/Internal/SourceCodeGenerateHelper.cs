using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Internal;

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
    public static void GenerateInstanceMethodProxyBodyCode(ClassStringBuilder builder, JuxtaposeContextSourceGeneratorContext context, ResourceCollection resources, IMethodSymbol method, VariableName vars)
    {
        var isAwaitable = context.TypeSymbolAnalyzer.IsAwaitable(method.ReturnType);
        var commandId = context.Resources.GetCommandId(method);

        builder.AppendLine(vars.MethodBodyPrefixSnippet);

        builder.AppendIndentLine($"{vars.RunningToken}.ThrowIfCancellationRequested();");

        var cancellationTokenParams = new List<CancellationTokenArgumentInfo>(1);
        var delegateParams = new List<DelegateArgumentInfo>(1);

        for (int index = 0; index < method.Parameters.Length; index++)
        {
            var parameter = method.Parameters[index];

            if (context.TypeSymbolAnalyzer.IsCancellationToken(parameter.Type))
            {
                cancellationTokenParams.Add(new(parameter, index + 1));
                builder.AppendIndentLine($"{parameter.Name}.ThrowIfCancellationRequested();");
                builder.AppendIndentLine($"{TypeFullNames.Juxtapose.ReferenceId}? {parameter.Name}_RID = {parameter.Name}.CanBeCanceled ? {vars.Executor}.InstanceIdGenerator.Next() : null;");
            }

            if (parameter.Type.IsDelegate())
            {
                delegateParams.Add(new(parameter, index + 1));
                var callbackMethod = ((INamedTypeSymbol)parameter.Type).DelegateInvokeMethod!;

                var callbackBodyBuilder = new ClassStringBuilder();
                callbackBodyBuilder.Indent();
                callbackBodyBuilder.Indent();
                GenerateMethodInvokeThroughMessageCode(context, resources, callbackBodyBuilder, callbackMethod, new VariableName(vars) { Instance = parameter.Name });

                builder.AppendLine($@"{TypeFullNames.Juxtapose.ReferenceId}? {parameter.Name}_RID = null;
if ({parameter.Name} is not null
    && !{vars.Executor}.IsDisposed)
{{
    {parameter.Name}_RID = {vars.Executor}.InstanceIdGenerator.Next();
    {vars.Executor}.AddObjectInstance({parameter.Name}_RID.Value, new {(isAwaitable ? "Async" : "Sync")}DelegateMessageExecutor({(isAwaitable ? "async " : string.Empty)}(exector, {vars.Message}) =>
    {{
{callbackBodyBuilder.ToString().Trim('\r', '\n')}
    }}));
}}");
            }
        }

        if (cancellationTokenParams.Count > 0)
        {
            builder.AppendIndentLine("bool shouldSendCancel = false;");
        }

        if (cancellationTokenParams.Count > 0
            || delegateParams.Count > 0)
        {
            builder.AppendLine(@"try
{");
            builder.Indent();
        }

        ArgumentsAndResultsHelper.GenerateMethodArgumentsPackCode(method, context.TypeSymbolAnalyzer, builder, "parameterPack");

        if (cancellationTokenParams.Count > 0)
        {
            builder.AppendIndentLine($"using var localCts = CancellationTokenSource.CreateLinkedTokenSource({vars.RunningToken}, {string.Join(", ", cancellationTokenParams.Select(m => m.ParameterSymbol.Name))});");
            builder.AppendIndentLine("localCts.Token.ThrowIfCancellationRequested();");
            builder.AppendIndentLine("var localToken = localCts.Token;");
            builder.AppendIndentLine("shouldSendCancel = true;");
        }
        else
        {
            builder.AppendIndentLine($"var localToken = {vars.RunningToken};");
        }

        var returnType = context.TypeSymbolAnalyzer.GetReturnType(method);
        if (returnType is not null)
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

        if (returnType is not null)
        {
            builder.Append($"<{ArgumentsAndResultsHelper.GenerateArgumentTypeName(method, context.TypeSymbolAnalyzer)}, {ArgumentsAndResultsHelper.GenerateValueTupleTypeName(new[] { returnType }, context.TypeSymbolAnalyzer)}>");
        }

        builder.Append($"(parameterPack, {vars.InstanceId}, (int){context.GetCommandAccessExpression(commandId)}, localToken)");

        if (returnType is not null)
        {
            builder.Append(").Item1;");
        }
        else
        {
            builder.Append(";");
        }

        builder.AppendLine();

        if (cancellationTokenParams.Count > 0
            || delegateParams.Count > 0)
        {
            builder.Dedent();

            builder.AppendLine(@"}
finally
{");
            builder.Indent();

            foreach (var parameter in delegateParams)
            {
                builder.AppendLine($@"if ({parameter.ParameterSymbol.Name}_RID.HasValue
    && !{vars.Executor}.IsDisposed)
{{
    {vars.Executor}.RemoveObjectInstance({parameter.ParameterSymbol.Name}_RID.Value);
}}");
            }

            foreach (var parameter in cancellationTokenParams)
            {
                builder.AppendLine($@"if (shouldSendCancel
    && {parameter.ParameterSymbol.Name}.IsCancellationRequested
    && !{vars.RunningToken}.IsCancellationRequested 
    && !{vars.Executor}.IsDisposed)
{{
    {(isAwaitable ? "await " : string.Empty)}{vars.Executor}.InvokeInstanceMethodMessage{(isAwaitable ? "Async" : string.Empty)}(CancellationTokenSourceCancelParameterPack.Instance, {parameter.ParameterSymbol.Name}_RID!.Value, (int){TypeFullNames.Juxtapose.SpecialCommand}.{nameof(SpecialCommand.CancelCancellationToken)},{vars.RunningToken});
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
    public static void GenerateStaticMethodProxyBodyCode(ClassStringBuilder builder, JuxtaposeContextSourceGeneratorContext context, ResourceCollection resources, IMethodSymbol method, VariableName vars)
    {
        var isAwaitable = context.TypeSymbolAnalyzer.IsAwaitable(method.ReturnType);
        var commandId = context.Resources.GetCommandId(method);

        var awaitTag = isAwaitable ? "await " : string.Empty;
        var asyncTag = isAwaitable ? "Async" : string.Empty;

        vars.Executor = "executorOwner.Executor";

        builder.AppendIndentLine($"IJuxtaposeExecutorOwner? executorOwner = null;");

        var cancellationTokenParams = new List<CancellationTokenArgumentInfo>(1);
        var delegateParams = new List<DelegateArgumentInfo>(1);

        for (int index = 0; index < method.Parameters.Length; index++)
        {
            var parameter = method.Parameters[index];

            if (context.TypeSymbolAnalyzer.IsCancellationToken(parameter.Type))
            {
                cancellationTokenParams.Add(new(parameter, index + 1));
                builder.AppendIndentLine($"{parameter.Name}.ThrowIfCancellationRequested();");
                builder.AppendIndentLine($"{TypeFullNames.Juxtapose.ReferenceId}? {parameter.Name}_RID = null;");
            }

            if (parameter.Type.IsDelegate())
            {
                delegateParams.Add(new(parameter, index + 1));
                builder.AppendIndentLine($"{TypeFullNames.Juxtapose.ReferenceId}? {parameter.Name}_RID = null;");
            }
        }

        if (cancellationTokenParams.Count > 0)
        {
            builder.AppendIndentLine("bool shouldSendCancel = false;");
        }

        builder.AppendLine(@"try
{");
        builder.Indent();

        if (cancellationTokenParams.Count > 0)
        {
            builder.AppendIndentLine($"using var localCts = CancellationTokenSource.CreateLinkedTokenSource({vars.RunningToken}, {string.Join(", ", cancellationTokenParams.Select(m => m.ParameterSymbol.Name))});");
            builder.AppendIndentLine("localCts.Token.ThrowIfCancellationRequested();");
            builder.AppendIndentLine("var localToken = localCts.Token;");
        }
        else
        {
            builder.AppendIndentLine($"var localToken = CancellationToken.None;");
        }

        builder.AppendIndentLine($"executorOwner = {awaitTag}s_context.GetExecutorOwner{asyncTag}({method.GetCreationContextVariableName()}, localToken);");

        foreach (var parameter in cancellationTokenParams)
        {
            builder.AppendIndentLine($"{parameter.ParameterSymbol.Name}_RID = {parameter.ParameterSymbol.Name}.CanBeCanceled ? {vars.Executor}.InstanceIdGenerator.Next() : null;");
        }

        foreach (var parameter in delegateParams)
        {
            var callbackMethod = ((INamedTypeSymbol)parameter.ParameterSymbol.Type).DelegateInvokeMethod!;

            var callbackBodyBuilder = new ClassStringBuilder();
            callbackBodyBuilder.Indent();
            callbackBodyBuilder.Indent();
            GenerateMethodInvokeThroughMessageCode(context, resources, callbackBodyBuilder, callbackMethod, new VariableName(vars)
            {
                Instance = parameter.ParameterSymbol.Name,
                InstanceId = $"{parameter.ParameterSymbol.Name}_RID.Value",
            });

            builder.AppendLine($@"if ({parameter.ParameterSymbol.Name} is not null
    && !{vars.Executor}.IsDisposed)
{{
    {parameter.ParameterSymbol.Name}_RID = {vars.Executor}.InstanceIdGenerator.Next();
    {vars.Executor}.AddObjectInstance({parameter.ParameterSymbol.Name}_RID.Value, new {(isAwaitable ? "Async" : "Sync")}DelegateMessageExecutor({(isAwaitable ? "async " : string.Empty)}(exector, {vars.Message}) =>
    {{
{callbackBodyBuilder.ToString().Trim('\r', '\n')}
    }}));
}}");
        }

        ArgumentsAndResultsHelper.GenerateMethodArgumentsPackCode(method, context.TypeSymbolAnalyzer, builder, "parameterPack");

        var returnType = context.TypeSymbolAnalyzer.GetReturnType(method);
        if (returnType is not null)
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

        if (returnType is not null)
        {
            builder.Append($"<{ArgumentsAndResultsHelper.GenerateArgumentTypeName(method, context.TypeSymbolAnalyzer)}, {ArgumentsAndResultsHelper.GenerateValueTupleTypeName(new[] { returnType }, context.TypeSymbolAnalyzer)}>");
        }

        builder.Append($"(parameterPack, (int){context.GetCommandAccessExpression(commandId)}, localToken)");

        if (returnType is not null)
        {
            builder.Append(").Item1;");
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
            foreach (var parameter in delegateParams)
            {
                builder.AppendLine($@"if ({parameter.ParameterSymbol.Name}_RID.HasValue
    && !{vars.Executor}.IsDisposed)
{{
    {vars.Executor}.RemoveObjectInstance({parameter.ParameterSymbol.Name}_RID.Value);
}}");
            }

            foreach (var parameter in cancellationTokenParams)
            {
                builder.AppendLine($@"if (shouldSendCancel
&& {parameter.ParameterSymbol.Name}.IsCancellationRequested
&& !cancellation.IsCancellationRequested
&& !{vars.Executor}.IsDisposed)
{{
    {awaitTag}{vars.Executor}.InvokeStaticMethodMessage{asyncTag}(CancellationTokenSourceCancelParameterPack.Instance, (int){TypeFullNames.Juxtapose.SpecialCommand}.{nameof(SpecialCommand.CancelCancellationToken)}, {vars.Executor}.RunningToken);
}}");
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
    public static void GenerateMethodInvokeThroughMessageCaseScopeCode(JuxtaposeContextSourceGeneratorContext context, ResourceCollection resources, ClassStringBuilder sourceBuilder, IMethodSymbol method, VariableName vars)
    {
        var methodInvokeMessageTypeName = GetInvokeMessageFullTypeName(method, context.TypeSymbolAnalyzer);

        var commandId = context.Resources.GetCommandId(method);

        sourceBuilder.AppendIndentLine($"case (int){GeneratedCommandUtil.GetCommandAccessExpression(context, commandId)}:");

        sourceBuilder.Indent();

        sourceBuilder.Scope(() =>
        {
            GenerateMethodInvokeThroughMessageCode(context, resources, sourceBuilder, method, vars);
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
    public static void GenerateMethodInvokeThroughMessageCode(JuxtaposeContextSourceGeneratorContext context, ResourceCollection resources, ClassStringBuilder sourceBuilder, IMethodSymbol method, VariableName vars)
    {
        var methodInvokeMessageTypeName = GetInvokeMessageFullTypeName(method, context.TypeSymbolAnalyzer);

        var returnType = context.TypeSymbolAnalyzer.GetReturnType(method);
        var methodInvokeResultMessageTypeName = GetInvokeResultMessageFullTypeName(method, context.TypeSymbolAnalyzer, returnType);

        sourceBuilder.AppendIndentLine($"var ___typedMessage__ = ({methodInvokeMessageTypeName}){vars.Message};");

        ArgumentsAndResultsHelper.GenerateMethodArgumentsUnpackCode(method, context, resources, sourceBuilder, () =>
        {
            if (method.MethodKind == MethodKind.PropertyGet)
            {
                var propName = method.Name.Replace("get_", string.Empty).Replace("set_", string.Empty);
                if (method.IsStatic)
                {
                    sourceBuilder.AppendIndentLine($"return new {methodInvokeResultMessageTypeName}({vars.Message}.Id) {{ Result = new({method.ContainingType.ToFullyQualifiedDisplayString()}.{propName}) }};");
                }
                else
                {
                    sourceBuilder.AppendIndentLine($"return new {methodInvokeResultMessageTypeName}({vars.Message}.Id, {vars.InstanceId}) {{ Result = new({vars.Instance}.{propName}) }};");
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

            if (context.TypeSymbolAnalyzer.IsReturnVoidOrTask(method))
            {
                sourceBuilder.AppendIndentSpace();
            }
            else
            {
                sourceBuilder.AppendIndent("var result = ");
            }

            if (context.TypeSymbolAnalyzer.IsAwaitable(method.ReturnType))
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

            if (context.TypeSymbolAnalyzer.IsReturnVoidOrTask(method))
            {
                sourceBuilder.AppendIndentLine("return null;");
            }
            else
            {
                if (method.IsStatic)
                {
                    sourceBuilder.AppendIndentLine($"return new {methodInvokeResultMessageTypeName}({vars.Message}.Id) {{ Result = new(result) }};");
                }
                else
                {
                    sourceBuilder.AppendIndentLine($"return new {methodInvokeResultMessageTypeName}({vars.Message}.Id, {vars.InstanceId}) {{ Result = new(result) }};");
                }
            }
        }, new VariableName(vars)
        {
            ParameterPack = "___typedMessage__.ParameterPack!",
        });
    }

    #endregion Public 方法

    #region Private 方法

    private static string GetInvokeMessageFullTypeName(IMethodSymbol method, TypeSymbolAnalyzer typeSymbolAnalyzer)
    {
        var valueTypeName = ArgumentsAndResultsHelper.GenerateArgumentTypeName(method, typeSymbolAnalyzer);
        return method.IsStatic
               ? $"{TypeFullNames.Juxtapose.Messages.StaticMethodInvokeMessage}<{valueTypeName}>"
               : $"{TypeFullNames.Juxtapose.Messages.InstanceMethodInvokeMessage}<{valueTypeName}>";
    }

    private static string GetInvokeResultMessageFullTypeName(IMethodSymbol method, TypeSymbolAnalyzer typeSymbolAnalyzer, ITypeSymbol? returnTypeSymbol)
    {
        if (returnTypeSymbol == null)
        {
            return string.Empty;
        }
        var valueTypeName = ArgumentsAndResultsHelper.GenerateValueTupleTypeName(new[] { returnTypeSymbol }, typeSymbolAnalyzer);
        return method.IsStatic
               ? $"{TypeFullNames.Juxtapose.Messages.StaticMethodInvokeResultMessage}<{valueTypeName}>"
               : $"{TypeFullNames.Juxtapose.Messages.InstanceMethodInvokeResultMessage}<{valueTypeName}>";
    }

    #endregion Private 方法
}
