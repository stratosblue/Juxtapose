using Juxtapose.SourceGenerator.Internal;
using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate;

/// <summary>
/// 实例代理代码生成器
/// </summary>
public class InstanceProxyCodeGenerator : ProxyCodeGenerator
{
    #region Private 字段

    private readonly VariableName _vars;

    #endregion Private 字段

    #region Public 构造函数

    public InstanceProxyCodeGenerator(JuxtaposeContextSourceGeneratorContext context,
                                      ResourceCollection resources,
                                      ClassStringBuilder sourceBuilder,
                                      INamedTypeSymbol typeSymbol,
                                      VariableName vars)
        : base(context, resources, sourceBuilder, typeSymbol)
    {
        _vars = vars ?? throw new ArgumentNullException(nameof(vars));
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override void GenerateMethodProxyBody(ClassStringBuilder builder, IMethodSymbol method)
    {
        SourceCodeGenerateHelper.GenerateInstanceMethodProxyBodyCode(builder, Context, Resources, method, _vars);
    }

    protected override string GenerateMethodProxyCode(IMethodSymbol methodSymbol)
    {
        var builder = new ClassStringBuilder();

        var returnType = methodSymbol.ReturnType;

        var methodAnnotation = $"""
                        /// <summary>
                        /// <inheritdoc cref="{methodSymbol.ToInheritDocCrefString()}"/>
                        /// <br/><br/>invoke remote method <see cref="{methodSymbol.ToInheritDocCrefString()}"/>
                        /// </summary>
                """;
        builder.AppendIndentLine(methodAnnotation);

        builder.AppendIndentLine($"public {(TypeSymbolAnalyzer.IsAwaitable(returnType) ? "async " : string.Empty)}{returnType.ToFullyQualifiedDisplayString()} {methodSymbol.Name}({methodSymbol.GenerateMethodArgumentString()})");
        builder.Scope(() =>
        {
            GenerateMethodProxyBody(builder, methodSymbol);
        });

        return builder.ToString();
    }

    protected override string GeneratePropertyProxyCode(IPropertySymbol propertySymbol)
    {
        var builder = new ClassStringBuilder();

        builder.AppendIndentLine($"/// <inheritdoc cref=\"global::{propertySymbol.ToInheritDocCrefString()}\"/>");

        builder.AppendIndentLine($"public {propertySymbol.Type.ToFullyQualifiedDisplayString()} {propertySymbol.Name}");
        builder.Scope(() =>
        {
            if (propertySymbol.GetMethod is not null)
            {
                builder.AppendIndentLine("get");
                builder.Scope(() =>
                {
                    GenerateMethodProxyBody(builder, propertySymbol.GetMethod);
                });
            }

            if (propertySymbol.SetMethod is not null)
            {
                builder.AppendIndentLine("set");
                builder.Scope(() =>
                {
                    GenerateMethodProxyBody(builder, propertySymbol.SetMethod);
                });
            }
        });
        return builder.ToString();
    }

    #endregion Protected 方法
}
