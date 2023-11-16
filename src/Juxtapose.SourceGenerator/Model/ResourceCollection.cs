using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

public abstract class ResourceCollection
{
    #region Private 字段

    private readonly ConcurrentDictionary<ISymbol, int> _commandIdMap = new(SymbolEqualityComparer.Default);

    /// <summary>
    /// 指令计数
    /// </summary>
    private int _commandIndex = byte.MaxValue;

    #endregion Private 字段

    #region Protected 属性

    /// <summary>
    /// 构造函数集合
    /// </summary>
    protected ConcurrentDictionary<IMethodSymbol, int> Constructors { get; } = new(SymbolEqualityComparer.Default);

    /// <summary>
    /// 委托集合
    /// </summary>
    protected ConcurrentDictionary<IMethodSymbol, int> Delegates { get; private set; } = new(SymbolEqualityComparer.Default);

    /// <summary>
    /// 方法集合
    /// </summary>
    protected ConcurrentDictionary<IMethodSymbol, int> Methods { get; private set; } = new(SymbolEqualityComparer.Default);

    /// <summary>
    /// 属性集合
    /// </summary>
    protected ConcurrentDictionary<IPropertySymbol, int?> Properties { get; private set; } = new(SymbolEqualityComparer.Default);

    /// <summary>
    /// 类型-RealObjectInvoker源码 映射
    /// </summary>
    protected ConcurrentDictionary<INamedTypeSymbol, RealObjectInvokerSourceCode> RealObjectInvokers { get; private set; } = new(SymbolEqualityComparer.Default);

    protected ConcurrentBag<SourceCode> SourceCodes { get; } = new();

    #endregion Protected 属性

    #region Public 属性

    public TypeSymbolAnalyzer TypeSymbolAnalyzer { get; }

    #endregion Public 属性

    #region Public 构造函数

    public ResourceCollection(TypeSymbolAnalyzer typeSymbolAnalyzer)
    {
        TypeSymbolAnalyzer = typeSymbolAnalyzer;
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected virtual int GetOrCreateCommandId(ISymbol symbol)
    {
        return _commandIdMap.GetOrAdd(symbol, _ => Interlocked.Increment(ref _commandIndex));
    }

    #endregion Protected 方法

    #region Public 方法

    public virtual void AddConstructors(IMethodSymbol methodSymbol)
    {
        var id = GetOrCreateCommandId(methodSymbol);
        Constructors.TryAdd(methodSymbol, id);
    }

    public virtual void AddDelegates(IMethodSymbol methodSymbol)
    {
        var id = GetOrCreateCommandId(methodSymbol);
        Delegates.TryAdd(methodSymbol, id);
    }

    public virtual void AddMethods(IMethodSymbol methodSymbol)
    {
        var id = GetOrCreateCommandId(methodSymbol);
        Methods.TryAdd(methodSymbol, id);
    }

    public virtual void AddProperties(IPropertySymbol propertySymbol)
    {
        Properties.TryAdd(propertySymbol, Properties.Count);
    }

    public virtual void AddSourceCode(SourceCode sourceCode)
    {
        SourceCodes.Add(sourceCode);
    }

    public virtual IEnumerable<ISymbol> GetAllCommandedSymbol() => GetAllConstructors().Concat(GetAllMethods()).Concat(GetAllDelegates());

    public virtual IEnumerable<IMethodSymbol> GetAllConstructors() => Constructors.OrderBy(m => m.Value).Select(m => m.Key);

    public virtual IEnumerable<IMethodSymbol> GetAllDelegates() => Delegates.OrderBy(m => m.Value).Select(m => m.Key);

    public virtual IEnumerable<IMethodSymbol> GetAllMethods() => Methods.OrderBy(m => m.Value).Select(m => m.Key);

    public virtual IEnumerable<IPropertySymbol> GetAllProperties() => Properties.OrderBy(m => m.Value).Select(m => m.Key);

    public virtual int GetCommandId(ISymbol symbol) => _commandIdMap[symbol];

    public virtual bool TryAddRealObjectInvokerSourceCode(INamedTypeSymbol targetTypeSymbol, RealObjectInvokerSourceCode invokerSourceCode)
    {
        return RealObjectInvokers.TryAdd(targetTypeSymbol, invokerSourceCode);
    }

    public virtual bool TryGetRealObjectInvokerSourceCode(INamedTypeSymbol targetTypeSymbol, out RealObjectInvokerSourceCode? invokerSourceCode)
    {
        return RealObjectInvokers.TryGetValue(targetTypeSymbol, out invokerSourceCode);
    }

    #endregion Public 方法
}
