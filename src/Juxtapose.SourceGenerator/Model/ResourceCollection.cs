using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model
{
    public abstract class ResourceCollection
    {
        #region Protected 属性

        /// <summary>
        /// 构造函数参数包
        /// </summary>
        protected Dictionary<IMethodSymbol, Dictionary<string, ConstructorParameterPackSourceCode>> ConstructorParameterPacks { get; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 委托参数包字典
        /// </summary>
        protected Dictionary<IMethodSymbol, ParameterPackSourceCode> DelegateParameterPacks { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 委托返回值包字典
        /// </summary>
        protected Dictionary<IMethodSymbol, ResultPackSourceCode?> DelegateResultPacks { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 方法参数包字典
        /// </summary>
        protected Dictionary<IMethodSymbol, ParameterPackSourceCode> MethodParameterPacks { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 方法返回值包字典
        /// </summary>
        protected Dictionary<IMethodSymbol, ResultPackSourceCode?> MethodResultPacks { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 类型-RealObjectInvoker源码 隐射
        /// </summary>
        protected Dictionary<INamedTypeSymbol, Dictionary<INamedTypeSymbol, RealObjectInvokerSourceCode>> RealObjectInvokers { get; private set; } = new(SymbolEqualityComparer.Default);

        protected List<SourceCode> SourceCodes { get; } = new();

        #endregion Protected 属性

        #region Public 方法

        public void AddSourceCode(SourceCode sourceCode)
        {
            SourceCodes.Add(sourceCode);
        }

        public IEnumerable<ConstructorParameterPackSourceCode> GetAllConstructorParameterPacks()
        {
            foreach (var map in ConstructorParameterPacks.Values)
            {
                foreach (var item in map.Values)
                {
                    yield return item;
                }
            }
        }

        public IEnumerable<ParameterPackSourceCode> GetAllDelegateParameterPacks()
        {
            foreach (var item in DelegateParameterPacks.Values)
            {
                yield return item;
            }
        }

        public IEnumerable<ResultPackSourceCode> GetAllDelegateResultPacks()
        {
            foreach (var item in DelegateResultPacks.Values)
            {
                if (item is not null)
                {
                    yield return item;
                }
            }
        }

        public IEnumerable<ParameterPackSourceCode> GetAllMethodParameterPacks()
        {
            foreach (var item in MethodParameterPacks.Values)
            {
                yield return item;
            }
        }

        public IEnumerable<ResultPackSourceCode> GetAllMethodResultPacks()
        {
            foreach (var item in MethodResultPacks.Values)
            {
                if (item is not null)
                {
                    yield return item;
                }
            }
        }

        public IEnumerable<IMethodSymbol> GetAllMethods()
        {
            foreach (var item in MethodParameterPacks.Keys)
            {
                yield return item;
            }
        }

        public virtual bool TryAddConstructorParameterPackSourceCode(ConstructorParameterPackSourceCode item)
        {
            if (!ConstructorParameterPacks.TryGetValue(item.MethodSymbol, out var map))
            {
                map = new();
                ConstructorParameterPacks.Add(item.MethodSymbol, map);
            }
            if (map.TryGetValue(item.GeneratedTypeName, out _))
            {
                return false;
            }
            map.Add(item.GeneratedTypeName, item);
            return true;
        }

        public virtual bool TryAddDelegateArgumentPackSourceCode(ArgumentPackSourceCode item)
        {
            var method = item.MethodSymbol;
            switch (item)
            {
                case ParameterPackSourceCode parameterPackSourceCode:
                    {
                        if (!DelegateParameterPacks.ContainsKey(method))
                        {
                            DelegateParameterPacks.Add(method, parameterPackSourceCode);
                            return true;
                        }
                    }
                    break;

                case ResultPackSourceCode resultPackSourceCode:
                    {
                        if (!DelegateResultPacks.ContainsKey(method))
                        {
                            DelegateResultPacks.Add(method, resultPackSourceCode);
                            return true;
                        }
                    }
                    break;

                default:
                    throw new NotSupportedException($"not support {item}");
            }
            return false;
        }

        public virtual IEnumerable<ArgumentPackSourceCode> TryAddMethodArgumentPackSourceCode(IEnumerable<ArgumentPackSourceCode> items)
        {
            foreach (var item in items)
            {
                if (TryAddMethodArgumentPackSourceCode(item))
                {
                    yield return item;
                }
            }
        }

        public virtual bool TryAddMethodArgumentPackSourceCode(ArgumentPackSourceCode item)
        {
            var method = item.MethodSymbol;
            switch (item)
            {
                case ParameterPackSourceCode parameterPackSourceCode:
                    {
                        if (!MethodParameterPacks.ContainsKey(method))
                        {
                            MethodParameterPacks.Add(method, parameterPackSourceCode);
                            return true;
                        }
                    }
                    break;

                case ResultPackSourceCode resultPackSourceCode:
                    {
                        if (!MethodResultPacks.ContainsKey(method))
                        {
                            MethodResultPacks.Add(method, resultPackSourceCode);
                            return true;
                        }
                    }
                    break;

                default:
                    throw new NotSupportedException($"not support {item}");
            }
            return false;
        }

        public virtual bool TryAddRealObjectInvokerSourceCode(INamedTypeSymbol targetTypeSymbol, INamedTypeSymbol? inheritTypeSymbol, RealObjectInvokerSourceCode invokerSourceCode)
        {
            inheritTypeSymbol ??= BuildEnvironment.VoidSymbol;
            if (RealObjectInvokers.TryGetValue(targetTypeSymbol, out var invokerSourceCodes))
            {
                invokerSourceCodes.Add(inheritTypeSymbol, invokerSourceCode);
            }
            else
            {
                invokerSourceCodes = new(SymbolEqualityComparer.Default);
                invokerSourceCodes.Add(inheritTypeSymbol, invokerSourceCode);

                RealObjectInvokers.Add(targetTypeSymbol, invokerSourceCodes);
            }
            return true;
        }

        public virtual bool TryGetConstructorParameterPackSourceCode(IMethodSymbol constructor, string generatedTypeName, out ConstructorParameterPackSourceCode? item)
        {
            if (ConstructorParameterPacks.TryGetValue(constructor, out var map)
                && map is not null
                && map.TryGetValue(generatedTypeName, out item))
            {
                return true;
            }
            item = default;
            return false;
        }

        public virtual bool TryGetMethodArgumentPackSourceCode(IMethodSymbol methodSymbol, out ParameterPackSourceCode? item)
        {
            return MethodParameterPacks.TryGetValue(methodSymbol, out item)
                   || DelegateParameterPacks.TryGetValue(methodSymbol, out item);
        }

        public virtual bool TryGetMethodResultPackSourceCode(IMethodSymbol methodSymbol, out ResultPackSourceCode? item)
        {
            return MethodResultPacks.TryGetValue(methodSymbol, out item)
                   || DelegateResultPacks.TryGetValue(methodSymbol, out item);
        }

        public virtual bool TryGetRealObjectInvokerSourceCode(INamedTypeSymbol targetTypeSymbol, INamedTypeSymbol? inheritTypeSymbol, out RealObjectInvokerSourceCode? invokerSourceCode)
        {
            inheritTypeSymbol ??= BuildEnvironment.VoidSymbol;
            invokerSourceCode = RealObjectInvokers.TryGetValue(targetTypeSymbol, out var invokerSourceCodes)
                                && invokerSourceCodes.TryGetValue(inheritTypeSymbol, out var sourceCode)
                                && sourceCode is not null
                                ? sourceCode
                                : null;
            return invokerSourceCode is not null;
        }

        #endregion Public 方法
    }
}