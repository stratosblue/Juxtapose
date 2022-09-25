using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

public class SubResourceCollection : ResourceCollection
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

    public override bool TryAddConstructorParameterPackSourceCode(ConstructorParameterPackSourceCode item)
    {
        _contextResourceCollection.TryAddConstructorParameterPackSourceCode(item);
        return base.TryAddConstructorParameterPackSourceCode(item);
    }

    public override bool TryAddDelegateArgumentPackSourceCode(ArgumentPackSourceCode item)
    {
        _contextResourceCollection.TryAddDelegateArgumentPackSourceCode(item);
        return base.TryAddDelegateArgumentPackSourceCode(item);
    }

    public override bool TryAddMethodArgumentPackSourceCode(ArgumentPackSourceCode item)
    {
        _contextResourceCollection.TryAddMethodArgumentPackSourceCode(item);
        return base.TryAddMethodArgumentPackSourceCode(item);
    }

    public override IEnumerable<ArgumentPackSourceCode> TryAddMethodArgumentPackSourceCode(IEnumerable<ArgumentPackSourceCode> items)
    {
        _contextResourceCollection.TryAddMethodArgumentPackSourceCode(items);
        return base.TryAddMethodArgumentPackSourceCode(items);
    }

    public override bool TryAddRealObjectInvokerSourceCode(INamedTypeSymbol targetTypeSymbol, INamedTypeSymbol? inheritTypeSymbol, RealObjectInvokerSourceCode invokerSourceCode)
    {
        _contextResourceCollection.TryAddRealObjectInvokerSourceCode(targetTypeSymbol, inheritTypeSymbol, invokerSourceCode);
        return base.TryAddRealObjectInvokerSourceCode(targetTypeSymbol, inheritTypeSymbol, invokerSourceCode);
    }

    #endregion Public 方法
}