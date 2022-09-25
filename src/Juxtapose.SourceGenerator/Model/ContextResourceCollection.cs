using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

public class ContextResourceCollection : ResourceCollection
{
    public ContextResourceCollection(TypeSymbolAnalyzer typeSymbolAnalyzer) : base(typeSymbolAnalyzer)
    {
    }
}