using System;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

/// <summary>
/// 构造函数参数包源代码
/// </summary>
public class ConstructorParameterPackSourceCode : ParameterPackSourceCode
{
    #region Public 属性

    /// <summary>
    /// 目标构造类型名称
    /// </summary>
    public string GeneratedTypeName { get; }

    #endregion Public 属性

    #region Public 构造函数

    public ConstructorParameterPackSourceCode(IMethodSymbol methodSymbol, string hintName, string source, string @namespace, string typeName, string typeFullName, string generatedTypeName)
        : base(methodSymbol, hintName, source, @namespace, typeName, typeFullName)
    {
        if (string.IsNullOrWhiteSpace(generatedTypeName))
        {
            throw new ArgumentException($"“{nameof(generatedTypeName)}”不能为 null 或空白。", nameof(generatedTypeName));
        }

        GeneratedTypeName = generatedTypeName;
    }

    #endregion Public 构造函数
}