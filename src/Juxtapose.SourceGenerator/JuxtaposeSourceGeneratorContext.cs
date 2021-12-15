using System;
using System.Collections.Generic;
using System.Linq;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

#pragma warning disable RS1024 // 正确比较符号

namespace Juxtapose.SourceGenerator
{
    public class JuxtaposeSourceGeneratorContext
    {
        #region Protected 属性

        /// <summary>
        /// 方法参数包字典
        /// </summary>
        protected Dictionary<IMethodSymbol, ParameterPackSourceCode> MethodParameterPacks { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 所有构造函数的参数包
        /// </summary>
        protected Dictionary<INamedTypeSymbol, HashSet<ParameterPackSourceCode>> TypeConstructorParameterPacks { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 类型的真实调用器源码字典
        /// </summary>
        protected Dictionary<INamedTypeSymbol, Dictionary<INamedTypeSymbol, RealObjectInvokerSourceCode>> TypeRealObjectInvokers { get; private set; } = new(SymbolEqualityComparer.Default);

        #endregion Protected 属性

        #region Public 属性

        /// <summary>
        /// 所有的构造函数
        /// </summary>
        public Dictionary<INamedTypeSymbol, HashSet<IMethodSymbol>> ConstructorMethods { get; private set; } = new(SymbolEqualityComparer.Default);

        public GeneratorExecutionContext GeneratorExecutionContext { get; }

        /// <summary>
        /// 实现类型-继承基类 字典
        /// </summary>
        public Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> ImplementInherits { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 继承基类-实现类型 字典
        /// </summary>
        public Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> InheritImplements { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 接口的方法列表
        /// </summary>
        public Dictionary<INamedTypeSymbol, HashSet<IMethodSymbol>> InterfaceMethods { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 方法返回值包字典
        /// </summary>
        public Dictionary<IMethodSymbol, ResultPackSourceCode?> MethodResultPacks { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 所有的静态方法
        /// </summary>
        public Dictionary<INamedTypeSymbol, HashSet<IMethodSymbol>> StaticMethods { get; private set; } = new(SymbolEqualityComparer.Default);

        #endregion Public 属性

        #region Public 构造函数

        public JuxtaposeSourceGeneratorContext(GeneratorExecutionContext generatorExecutionContext)
        {
            GeneratorExecutionContext = generatorExecutionContext;
        }

        #endregion Public 构造函数

        #region Public 方法

        #region GetInfos

        /// <summary>
        /// 获取所有构造函数的参数包
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ParameterPackSourceCode> GetAllConstructorParameterPacks()
        {
            foreach (var constructorParameterPacks in TypeConstructorParameterPacks)
            {
                foreach (var item in constructorParameterPacks.Value)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// 获取所有的参数包源码
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ParameterPackSourceCode> GetAllParameterPacks()
        {
            foreach (var item in MethodParameterPacks)
            {
                yield return item.Value;
            }
        }

        /// <summary>
        /// 获取方法参数源码
        /// </summary>
        /// <param name="methodSymbol"></param>
        /// <returns></returns>
        public bool TryGetParameterPack(IMethodSymbol methodSymbol, out ParameterPackSourceCode? parameterPackSourceCode)
        {
            return MethodParameterPacks.TryGetValue(methodSymbol, out parameterPackSourceCode);
        }

        #endregion GetInfos

        public void Clear()
        {
            MethodParameterPacks = new(SymbolEqualityComparer.Default);
            MethodResultPacks = new(SymbolEqualityComparer.Default);
            InterfaceMethods = new(SymbolEqualityComparer.Default);
            ConstructorMethods = new(SymbolEqualityComparer.Default);
            StaticMethods = new(SymbolEqualityComparer.Default);
            TypeRealObjectInvokers = new(SymbolEqualityComparer.Default);
            InheritImplements = new(SymbolEqualityComparer.Default);
            ImplementInherits = new(SymbolEqualityComparer.Default);
            TypeConstructorParameterPacks = new(SymbolEqualityComparer.Default);
        }

        public bool TryAddConstructorMethod(INamedTypeSymbol typeSymbol, IMethodSymbol methodSymbol)
        {
            return TryAddMethodIntoCollection(ConstructorMethods, typeSymbol, methodSymbol);
        }

        public bool TryAddConstructorMethods(INamedTypeSymbol typeSymbol, IEnumerable<IMethodSymbol> constructorMethods)
        {
            return constructorMethods.Count(m => TryAddConstructorMethod(typeSymbol, m)) > 0;
        }

        public bool TryAddImplementInherit(INamedTypeSymbol implementTypeSymbol, INamedTypeSymbol? inheritTypeSymbol)
        {
            return TryAddTypeMap(InheritImplements, inheritTypeSymbol, implementTypeSymbol)
                   | TryAddTypeMap(ImplementInherits, implementTypeSymbol, inheritTypeSymbol);
        }

        public bool TryAddInterfaceMethod(INamedTypeSymbol interfaceTypeSymbol, IMethodSymbol methodSymbol)
        {
            return TryAddMethodIntoCollection(InterfaceMethods, interfaceTypeSymbol, methodSymbol);
        }

        public bool TryAddInterfaceMethods(INamedTypeSymbol interfaceTypeSymbol, IEnumerable<IMethodSymbol> methodSymbols)
        {
            return methodSymbols.Count(m => TryAddInterfaceMethod(interfaceTypeSymbol, m)) > 0;
        }

        public bool TryAddMethodArgumentPackSourceCode(ArgumentPackSourceCode item)
        {
            var method = item.MethodSymbol;
            switch (item)
            {
                case ParameterPackSourceCode parameterPackSourceCode:
                    {
                        if (!MethodParameterPacks.ContainsKey(method))
                        {
                            MethodParameterPacks.Add(method, parameterPackSourceCode);

                            if (method.MethodKind == MethodKind.Constructor)
                            {
                                var type = method.ContainingType;
                                if (TypeConstructorParameterPacks.TryGetValue(type, out var constructorParameterPacks))
                                {
                                    constructorParameterPacks.Add(parameterPackSourceCode);
                                }
                                else
                                {
                                    constructorParameterPacks = new(ParameterPackSourceCode.EqualityComparer) { parameterPackSourceCode };
                                    TypeConstructorParameterPacks.Add(type, constructorParameterPacks);
                                }
                            }

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

        public bool TryAddMethodArgumentPackSourceCodes(IEnumerable<ArgumentPackSourceCode> methodSymbols)
        {
            return methodSymbols.Count(m => TryAddMethodArgumentPackSourceCode(m)) > 0;
        }

        #region RealObjectInvoker

        /// <summary>
        /// 尝试添加类型-继承类型-Invoker源码到上下文中
        /// </summary>
        /// <param name="originTypeSymbol"></param>
        /// <param name="inheritTypeSymbol"></param>
        /// <param name="invokerSourceCode"></param>
        /// <returns></returns>
        public bool TryAddRealObjectInvokerSourceCode(INamedTypeSymbol originTypeSymbol, INamedTypeSymbol inheritTypeSymbol, RealObjectInvokerSourceCode invokerSourceCode)
        {
            if (TypeRealObjectInvokers.TryGetValue(originTypeSymbol, out var invokerSourceCodes))
            {
                invokerSourceCodes.Add(inheritTypeSymbol, invokerSourceCode);
            }
            else
            {
                invokerSourceCodes = new(SymbolEqualityComparer.Default);
                invokerSourceCodes.Add(inheritTypeSymbol, invokerSourceCode);

                TypeRealObjectInvokers.Add(originTypeSymbol, invokerSourceCodes);
            }
            return true;
        }

        public bool TryGetRealObjectInvokerSourceCode(INamedTypeSymbol originTypeSymbol, INamedTypeSymbol? inheritTypeSymbol, out RealObjectInvokerSourceCode? invokerSourceCode)
        {
            inheritTypeSymbol ??= BuildEnvironment.VoidSymbol;
            if (TypeRealObjectInvokers.TryGetValue(originTypeSymbol, out var invokerSourceCodes)
                && invokerSourceCodes.TryGetValue(inheritTypeSymbol, out invokerSourceCode))
            {
                return true;
            }
            else
            {
                invokerSourceCode = null;
                return false;
            }
        }

        #endregion RealObjectInvoker

        public bool TryAddStaticMethod(INamedTypeSymbol staticTypeSymbol, IMethodSymbol methodSymbol)
        {
            return TryAddMethodIntoCollection(StaticMethods, staticTypeSymbol, methodSymbol);
        }

        public bool TryAddStaticMethods(INamedTypeSymbol staticTypeSymbol, IEnumerable<IMethodSymbol> methodSymbols)
        {
            return methodSymbols.Count(m => TryAddStaticMethod(staticTypeSymbol, m)) > 0;
        }

        #endregion Public 方法

        #region Private 方法

        private static bool TryAddMethodIntoCollection(Dictionary<INamedTypeSymbol, HashSet<IMethodSymbol>> collection, INamedTypeSymbol typeSymbol, IMethodSymbol methodSymbol)
        {
            if (methodSymbol.ContainingType != typeSymbol)
            {
                return false;
            }
            if (collection.TryGetValue(typeSymbol, out var methods))
            {
                return methods.Add(methodSymbol);
            }
            else
            {
                methods = new(SymbolEqualityComparer.Default);
                methods.Add(methodSymbol);
                collection.Add(typeSymbol, methods);
            }
            return true;
        }

        private static bool TryAddTypeMap(Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> collection, INamedTypeSymbol fromTypeSymbol, INamedTypeSymbol toTypeSymbol)
        {
            if (collection.TryGetValue(fromTypeSymbol, out var implements))
            {
                return implements.Add(toTypeSymbol);
            }
            else
            {
                implements = new(SymbolEqualityComparer.Default);
                implements.Add(toTypeSymbol);

                collection.Add(fromTypeSymbol, implements);
            }
            return true;
        }

        #endregion Private 方法
    }
}