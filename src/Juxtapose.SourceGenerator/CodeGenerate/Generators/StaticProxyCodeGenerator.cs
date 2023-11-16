using Juxtapose.SourceGenerator.Internal;
using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate;

/// <summary>
/// 静态代理代码生成器
/// </summary>
public class StaticProxyCodeGenerator : ProxyCodeGenerator
{
    #region Private 字段

    private readonly VariableName _vars;

    #endregion Private 字段

    #region Public 构造函数

    public StaticProxyCodeGenerator(JuxtaposeContextSourceGeneratorContext context,
                                    ResourceCollection resources,
                                    ClassStringBuilder sourceBuilder,
                                    INamedTypeSymbol typeSymbol,
                                    VariableName vars)
        : base(context, resources, sourceBuilder, typeSymbol)
    {
        _vars = vars ?? throw new ArgumentNullException(nameof(vars));
    }

    #endregion Public 构造函数

    #region Private 方法

    private void GenerateCreationContext(IMethodSymbol? methodSymbol)
    {
        if (methodSymbol is null)
        {
            return;
        }

        Builder.AppendIndentLine($"private static readonly {TypeFullNames.Juxtapose.ExecutorCreationContext} {methodSymbol.GetCreationContextVariableName()} = new(typeof({methodSymbol.ContainingType.Name}), \"{methodSymbol.Name}\", true, true);", true);
    }

    #endregion Private 方法

    #region Protected 方法

    protected override void GenerateMethodProxyBody(ClassStringBuilder builder, IMethodSymbol method)
    {
        SourceCodeGenerateHelper.GenerateStaticMethodProxyBodyCode(builder, Context, Resources, method, new VariableName(_vars) { RunningToken = "CancellationToken.None" });
    }

    protected override string GenerateMethodProxyCode(IMethodSymbol methodSymbol)
    {
        var builder = new ClassStringBuilder();

        var returnType = methodSymbol.ReturnType;

        builder.AppendIndentLine($"/// <inheritdoc cref=\"global::{methodSymbol.ToInheritDocCrefString()}\"/>");

        builder.AppendIndentLine($"public static {(TypeSymbolAnalyzer.IsAwaitable(returnType) ? "async " : string.Empty)}{returnType.ToFullyQualifiedDisplayString()} {methodSymbol.Name}({methodSymbol.GenerateMethodArgumentString()})");
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

        builder.AppendIndentLine($"public static {propertySymbol.Type.ToFullyQualifiedDisplayString()} {propertySymbol.Name}");
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

    #region Public 方法

    public override void GenerateMemberProxyCode()
    {
        var properties = Resources.GetAllProperties().ToList();
        var methods = Resources.GetAllMethods().ToList();

        foreach (var propertySymbol in properties)
        {
            GenerateCreationContext(propertySymbol.GetMethod);
            GenerateCreationContext(propertySymbol.SetMethod);
        }

        foreach (var methodSymbol in methods)
        {
            if (methodSymbol.IsPropertyAccessor())
            {
                continue;
            }
            GenerateCreationContext(methodSymbol);
        }

        foreach (var propertySymbol in properties)
        {
            Builder.AppendLine();
            Builder.AppendLine(GeneratePropertyProxyCode(propertySymbol));
        }

        foreach (var methodSymbol in methods)
        {
            if (methodSymbol.IsPropertyAccessor())
            {
                continue;
            }
            Builder.AppendLine();
            Builder.AppendLine(GenerateMethodProxyCode(methodSymbol));
        }
    }

    #endregion Public 方法
}
