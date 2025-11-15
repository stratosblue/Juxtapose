using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

internal readonly struct CancellationTokenArgumentInfo
{
    #region Public 属性

    public int ArgumentIndex { get; }

    public IParameterSymbol ParameterSymbol { get; }

    #endregion Public 属性

    #region Public 构造函数

    public CancellationTokenArgumentInfo(IParameterSymbol parameterSymbol, int argumentIndex)
    {
        ParameterSymbol = parameterSymbol;
        ArgumentIndex = argumentIndex;
    }

    #endregion Public 构造函数
}
