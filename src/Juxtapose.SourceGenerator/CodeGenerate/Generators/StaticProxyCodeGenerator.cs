using System;
using System.Linq;

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

    public StaticProxyCodeGenerator(JuxtaposeSourceGeneratorContext context, ClassStringBuilder sourceBuilder, INamedTypeSymbol typeSymbol, VariableName vars)
        : base(context, sourceBuilder, typeSymbol)
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
        SourceCodeGenerateHelper.GenerateStaticMethodProxyBodyCode(builder, Context, method, new VariableName(_vars) { RunningToken = "CancellationToken.None" });
    }

    protected override string GenerateMethodProxyCode(IMethodSymbol methodSymbol)
    {
        var builder = new ClassStringBuilder();

        var returnType = methodSymbol.ReturnType;

        builder.AppendIndentLine($"/// <inheritdoc cref=\"global::{methodSymbol.ToInheritDocCrefString()}\"/>");

        builder.AppendIndentLine($"public static {(returnType.IsAwaitable() ? "async " : string.Empty)}{returnType.ToFullyQualifiedDisplayString()} {methodSymbol.Name}({methodSymbol.GenerateMethodArgumentString()})");
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
        var typeMembers = TypeSymbol.GetProxyableMembers(false).Where(m => m.DeclaredAccessibility == Accessibility.Public).ToArray();

        foreach (var typeMember in typeMembers)
        {
            switch (typeMember)
            {
                case IPropertySymbol propertySymbol:
                    GenerateCreationContext(propertySymbol.GetMethod);
                    GenerateCreationContext(propertySymbol.SetMethod);
                    break;

                case IMethodSymbol methodSymbol:
                    if (methodSymbol.MethodKind == MethodKind.PropertyGet
                        || methodSymbol.MethodKind == MethodKind.PropertySet)
                    {
                        continue;
                    }
                    GenerateCreationContext(methodSymbol);
                    break;

                default://暂时先不支持
                    throw new NotImplementedException();
            }
        }

        foreach (var typeMember in typeMembers)
        {
            switch (typeMember.Kind)
            {
                case SymbolKind.Property:
                    Builder.AppendLine();
                    Builder.AppendLine(GeneratePropertyProxyCode((IPropertySymbol)typeMember));
                    break;

                case SymbolKind.Method:
                    var methodSymbol = (IMethodSymbol)typeMember;
                    if (methodSymbol.MethodKind == MethodKind.PropertyGet
                        || methodSymbol.MethodKind == MethodKind.PropertySet)
                    {
                        continue;
                    }
                    Builder.AppendLine();
                    Builder.AppendLine(GenerateMethodProxyCode(methodSymbol));
                    break;

                case SymbolKind.Event:   //暂时先不支持
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }
    }

    #endregion Public 方法
}