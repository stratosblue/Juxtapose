using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

/// <summary>
/// 返回值包源代码
/// </summary>
public class ResultPackSourceCode : ArgumentPackSourceCode
{
    #region Public 构造函数

    public ResultPackSourceCode(IMethodSymbol methodSymbol, string hintName, string source, string @namespace, string typeName, string typeFullName)
        : base(methodSymbol, hintName, source, @namespace, typeName, typeFullName)
    {
    }

    #endregion Public 构造函数
}