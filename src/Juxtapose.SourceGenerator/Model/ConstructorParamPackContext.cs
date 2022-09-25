using System;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

public class ConstructorParamPackContext : MethodParamPackContext
{
    #region Public 属性

    public string GeneratedTypeName { get; }

    #endregion Public 属性

    #region Public 构造函数

    public ConstructorParamPackContext(IMethodSymbol methodSymbol, string generatedTypeName, TypeSymbolAnalyzer typeSymbolAnalyzer) : base(methodSymbol, typeSymbolAnalyzer)
    {
        if (string.IsNullOrWhiteSpace(generatedTypeName))
        {
            throw new ArgumentException($"“{nameof(generatedTypeName)}”不能为 null 或空白。", nameof(generatedTypeName));
        }

        GeneratedTypeName = generatedTypeName.NormalizeAsClassName();
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override string GenParamPackClassName()
    {
        return $"{MethodSymbol.GetNormalizeClassName()}_ctor_{GeneratedTypeName}_ParamPack";
    }

    #endregion Protected 方法
}