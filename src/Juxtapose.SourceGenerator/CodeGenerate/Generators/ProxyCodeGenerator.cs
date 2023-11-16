using Juxtapose.SourceGenerator.Model;
using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.CodeGenerate;

public abstract class ProxyCodeGenerator
{
    #region Protected 字段

    protected readonly ClassStringBuilder Builder;

    #endregion Protected 字段

    #region Public 属性

    public JuxtaposeContextSourceGeneratorContext Context { get; }

    public ResourceCollection Resources { get; }

    public INamedTypeSymbol TypeSymbol { get; }

    public TypeSymbolAnalyzer TypeSymbolAnalyzer => Context.TypeSymbolAnalyzer;

    #endregion Public 属性

    #region Public 构造函数

    public ProxyCodeGenerator(JuxtaposeContextSourceGeneratorContext context, ResourceCollection resources, ClassStringBuilder sourceBuilder, INamedTypeSymbol typeSymbol)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Resources = resources ?? throw new ArgumentNullException(nameof(resources));
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
        foreach (var propertySymbol in Resources.GetAllProperties())
        {
            Builder.AppendLine();
            Builder.AppendLine(GeneratePropertyProxyCode(propertySymbol));
        }

        foreach (var methodSymbol in Resources.GetAllMethods())
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
