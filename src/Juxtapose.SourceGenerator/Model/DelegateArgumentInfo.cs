using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

internal struct DelegateArgumentInfo
{
    #region Public 属性

    public int ArgumentIndex { get; }

    public IParameterSymbol ParameterSymbol { get; }

    #endregion Public 属性

    #region Public 构造函数

    public DelegateArgumentInfo(IParameterSymbol parameterSymbol, int argumentIndex)
    {
        ParameterSymbol = parameterSymbol;
        ArgumentIndex = argumentIndex;
    }

    #endregion Public 构造函数
}
