using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

public class SubResourceCollection : ContextResourceCollection
{
    #region Private 字段

    private readonly ContextResourceCollection _contextResourceCollection;

    #endregion Private 字段

    #region Public 构造函数

    public SubResourceCollection(ContextResourceCollection contextResourceCollection) : base(contextResourceCollection.TypeSymbolAnalyzer)
    {
        _contextResourceCollection = contextResourceCollection ?? throw new ArgumentNullException(nameof(contextResourceCollection));
    }

    #endregion Public 构造函数

    #region Public 方法

    public override void AddConstructors(IMethodSymbol methodSymbol)
    {
        _contextResourceCollection.AddConstructors(methodSymbol);
        base.AddConstructors(methodSymbol);
    }

    public override void AddDelegates(IMethodSymbol methodSymbol)
    {
        _contextResourceCollection.AddDelegates(methodSymbol);
        base.AddDelegates(methodSymbol);
    }

    public override void AddMethods(IMethodSymbol methodSymbol)
    {
        _contextResourceCollection.AddMethods(methodSymbol);
        base.AddMethods(methodSymbol);
    }

    public override void AddSourceCode(SourceCode sourceCode)
    {
        _contextResourceCollection.AddSourceCode(sourceCode);
        base.AddSourceCode(sourceCode);
    }

    public override int GetCommandId(ISymbol symbol)
    {
        return _contextResourceCollection.GetCommandId(symbol);
    }

    public override bool TryAddRealObjectInvokerSourceCode(INamedTypeSymbol targetTypeSymbol, RealObjectInvokerSourceCode invokerSourceCode)
    {
        _contextResourceCollection.TryAddRealObjectInvokerSourceCode(targetTypeSymbol, invokerSourceCode);
        return base.TryAddRealObjectInvokerSourceCode(targetTypeSymbol, invokerSourceCode);
    }

    public override bool TryGetRealObjectInvokerSourceCode(INamedTypeSymbol targetTypeSymbol, out RealObjectInvokerSourceCode? invokerSourceCode)
    {
        _contextResourceCollection.TryGetRealObjectInvokerSourceCode(targetTypeSymbol, out invokerSourceCode);
        return base.TryGetRealObjectInvokerSourceCode(targetTypeSymbol, out invokerSourceCode);
    }

    #endregion Public 方法
}
