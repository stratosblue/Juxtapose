using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

public class ContextResourceCollection : ResourceCollection
{
    #region Public 构造函数

    public ContextResourceCollection(TypeSymbolAnalyzer typeSymbolAnalyzer) : base(typeSymbolAnalyzer)
    {
    }

    #endregion Public 构造函数
}
