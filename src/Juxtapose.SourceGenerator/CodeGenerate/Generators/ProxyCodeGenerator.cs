using System;
using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate;

public abstract class ProxyCodeGenerator
{
    #region Protected 字段

    protected readonly ClassStringBuilder Builder;

    #endregion Protected 字段

    #region Public 属性

    public JuxtaposeSourceGeneratorContext Context { get; }

    public INamedTypeSymbol TypeSymbol { get; }

    public TypeSymbolAnalyzer TypeSymbolAnalyzer => Context.TypeSymbolAnalyzer;

    #endregion Public 属性

    #region Public 构造函数

    public ProxyCodeGenerator(JuxtaposeSourceGeneratorContext context, ClassStringBuilder sourceBuilder, INamedTypeSymbol typeSymbol)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Builder = sourceBuilder ?? throw new ArgumentNullException(nameof(sourceBuilder));
        TypeSymbol = typeSymbol ?? throw new ArgumentNullException(nameof(typeSymbol));
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected abstract void GenerateMethodProxyBody(ClassStringBuilder builder, IMethodSymbol method);

    protected abstract string GenerateMethodProxyCode(IMethodSymbol methodSymbol);

    protected abstract string GeneratePropertyProxyCode(IPropertySymbol propertySymbol);

    #endregion Protected 方法

    #region Public 方法

    public virtual void GenerateMemberProxyCode()
    {
        var typeMembers = TypeSymbol.GetProxyableMembers(false);

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