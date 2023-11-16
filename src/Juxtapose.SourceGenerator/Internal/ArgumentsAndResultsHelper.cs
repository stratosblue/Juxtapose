using System.Text;
using Juxtapose.SourceGenerator.Model;
using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Internal;

internal static class ArgumentsAndResultsHelper
{
    #region Public 方法

    public static string GenerateArgumentTypeName(IMethodSymbol methodSymbol, TypeSymbolAnalyzer typeSymbolAnalyzer)
    {
        return GenerateValueTupleTypeName(methodSymbol.Parameters.Select(m => m.Type), typeSymbolAnalyzer);
    }

    public static void GenerateMethodArgumentsPackCode(IMethodSymbol methodSymbol, TypeSymbolAnalyzer typeSymbolAnalyzer, ClassStringBuilder builder, string variableName)
    {
        var typeName = GenerateValueTupleTypeName(methodSymbol.Parameters.Select(m => m.Type), typeSymbolAnalyzer);
        if (methodSymbol.Parameters.IsEmpty)
        {
            builder.AppendIndentLine($"var {variableName} = new {typeName}();");
            return;
        }

        builder.AppendIndent($"var {variableName} = new {typeName}(");

        for (int index = 0; index < methodSymbol.Parameters.Length; index++)
        {
            var parameter = methodSymbol.Parameters[index];
            if (typeSymbolAnalyzer.IsCancellationToken(parameter.Type)
                || parameter.Type.IsDelegate())
            {
                builder.Append($"{parameter.Name}_RID");
            }
            else
            {
                builder.Append($"{parameter.Name}");
            }

            if (index < methodSymbol.Parameters.Length - 1)
            {
                builder.Append(", ");
            }
        }

        builder.Append(");");
        builder.AppendLine();
    }

    public static void GenerateMethodArgumentsUnpackCode(IMethodSymbol methodSymbol, JuxtaposeContextSourceGeneratorContext context, ResourceCollection resources, ClassStringBuilder builder, Action scopeBuildAction, VariableName vars)
    {
        if (methodSymbol.Parameters.IsEmpty)
        {
            scopeBuildAction();
            builder.AppendLine();
            return;
        }

        const string LocalParamPackVariableName = "@___paramPack__";
        builder.AppendIndentLine($"var {LocalParamPackVariableName} = {vars.ParameterPack};");

        var cancellationTokenParams = new List<CancellationTokenArgumentInfo>(1);

        for (int index = 0; index < methodSymbol.Parameters.Length; index++)
        {
            var parameter = methodSymbol.Parameters[index];

            var currentParameterValueAccessExpression = $"{LocalParamPackVariableName}.Item{index + 1}";

            if (context.TypeSymbolAnalyzer.IsCancellationToken(parameter.Type))
            {
                cancellationTokenParams.Add(new(parameter, index + 1));
                builder.AppendIndentLine($"global::Juxtapose.Utils.CancellationTokenInvokeUtil.TryRebuildCancellationTokenSource({currentParameterValueAccessExpression}, {vars.Executor}, out var {parameter.Name}_CTS, out var {parameter.Name});", true);
            }
            else if (parameter.Type.IsDelegate())
            {
                var delegateVars = new VariableName(vars)
                {
                    InstanceId = $"{LocalParamPackVariableName}.Item{index + 1}.Value",
                };

                var callbackMethod = ((INamedTypeSymbol)parameter.Type).DelegateInvokeMethod!;

                var callbackBodyBuilder = new ClassStringBuilder();
                callbackBodyBuilder.Indent();
                callbackBodyBuilder.Indent();
                SourceCodeGenerateHelper.GenerateInstanceMethodProxyBodyCode(callbackBodyBuilder, context, resources, callbackMethod, delegateVars);

                builder.AppendLine($@"{parameter.Type.ToFullyQualifiedDisplayString()} {parameter.Name} = null!;
if ({LocalParamPackVariableName}.Item{index + 1}.HasValue)
{{
    {parameter.Name} = {(context.TypeSymbolAnalyzer.IsAwaitable(callbackMethod.ReturnType) ? "async " : string.Empty)}({callbackMethod.GenerateMethodArgumentStringWithoutType()}) =>
    {{
{callbackBodyBuilder.ToString().Trim('\r', '\n')}
    }};
}}");
            }
            else
            {
                //HACK 支持其它特殊参数时需要处理
                builder.AppendLine($"var {parameter.Name} = {currentParameterValueAccessExpression};");
            }
        }

        if (cancellationTokenParams.Count > 0)
        {
            builder.AppendIndentLine("try");
            builder.AppendIndentLine("{");
            builder.Indent();
        }

        scopeBuildAction();

        if (cancellationTokenParams.Count > 0)
        {
            builder.Dedent();
            builder.AppendIndentLine("}");
            builder.AppendIndentLine("finally");
            builder.Scope(() =>
            {
                foreach (var argumentInfo in cancellationTokenParams)
                {
                    var currentParameterValueAccessExpression = $"{LocalParamPackVariableName}.Item{argumentInfo.ArgumentIndex}";

                    builder.AppendLine($@"if ({currentParameterValueAccessExpression}.HasValue)
{{
    {vars.Executor}.RemoveObjectInstance({currentParameterValueAccessExpression}.Value);
    {argumentInfo.ParameterSymbol.Name}_CTS!.Dispose();
}}");
                }
            });
        }
    }

    public static string GenerateValueTupleTypeName(IEnumerable<ITypeSymbol> types, TypeSymbolAnalyzer typeSymbolAnalyzer)
    {
        var count = types.Count();
        if (count > 0)
        {
            var builder = new StringBuilder("global::System.ValueTuple<", 128);

            var index = 0;
            foreach (var parameter in types)
            {
                if (typeSymbolAnalyzer.IsCancellationToken(parameter) || parameter.IsDelegate())
                {
                    builder.Append(TypeFullNames.Juxtapose.ReferenceId);
                    builder.Append('?');
                }
                else
                {
                    builder.Append(parameter.ToFullyQualifiedDisplayString());
                }
                if (++index < count)
                {
                    builder.Append(", ");
                }
            }

            builder.Append('>');
            return builder.ToString();
        }

        return "global::System.ValueTuple";
    }

    #endregion Public 方法
}
