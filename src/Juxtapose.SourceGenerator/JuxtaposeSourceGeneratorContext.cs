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
        #region Public 属性

        /// <summary>
        /// 所有的构造函数
        /// </summary>
        public Dictionary<INamedTypeSymbol, HashSet<IMethodSymbol>> ConstructorMethods { get; private set; } = new(SymbolEqualityComparer.Default);

        public GeneratorExecutionContext GeneratorExecutionContext { get; }

        /// <summary>
        /// 类型的接口字典
        /// </summary>
        public Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> ImplementInterfaces { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 接口的类型字典
        /// </summary>
        public Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> InterfaceImplements { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 接口的方法列表
        /// </summary>
        public Dictionary<INamedTypeSymbol, HashSet<IMethodSymbol>> InterfaceMethods { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 方法参数包字典
        /// </summary>
        public Dictionary<IMethodSymbol, ParameterPackSourceCode> MethodParameterPacks { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 方法返回值包字典
        /// </summary>
        public Dictionary<IMethodSymbol, ResultPackSourceCode?> MethodResultPacks { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 所有的静态方法
        /// </summary>
        public Dictionary<INamedTypeSymbol, HashSet<IMethodSymbol>> StaticMethods { get; private set; } = new(SymbolEqualityComparer.Default);

        /// <summary>
        /// 类型的真实调用器源码字典
        /// </summary>
        public Dictionary<INamedTypeSymbol, Dictionary<INamedTypeSymbol, RealObjectInvokerSourceCode>> TypeRealObjectInvokers { get; private set; } = new(SymbolEqualityComparer.Default);

        #endregion Public 属性

        #region Public 构造函数

        public JuxtaposeSourceGeneratorContext(GeneratorExecutionContext generatorExecutionContext)
        {
            GeneratorExecutionContext = generatorExecutionContext;
        }

        #endregion Public 构造函数

        #region Public 方法

        public void Clear()
        {
            MethodParameterPacks = new(SymbolEqualityComparer.Default);
            MethodResultPacks = new(SymbolEqualityComparer.Default);
            InterfaceMethods = new(SymbolEqualityComparer.Default);
            ConstructorMethods = new(SymbolEqualityComparer.Default);
            StaticMethods = new(SymbolEqualityComparer.Default);
            TypeRealObjectInvokers = new(SymbolEqualityComparer.Default);
            InterfaceImplements = new(SymbolEqualityComparer.Default);
            ImplementInterfaces = new(SymbolEqualityComparer.Default);
        }

        public bool TryAddConstructorMethod(INamedTypeSymbol typeSymbol, IMethodSymbol methodSymbol)
        {
            return TryAddMethodIntoCollection(ConstructorMethods, typeSymbol, methodSymbol);
        }

        public bool TryAddConstructorMethods(INamedTypeSymbol typeSymbol, IEnumerable<IMethodSymbol> constructorMethods)
        {
            return constructorMethods.Count(m => TryAddConstructorMethod(typeSymbol, m)) > 0;
        }

        public bool TryAddInterfaceImplement(INamedTypeSymbol interfaceTypeSymbol, INamedTypeSymbol implementTypeSymbol)
        {
            return TryAddTypeMap(InterfaceImplements, interfaceTypeSymbol, implementTypeSymbol)
                   | TryAddTypeMap(ImplementInterfaces, implementTypeSymbol, interfaceTypeSymbol);
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
            switch (item)
            {
                case ParameterPackSourceCode parameterPackSourceCode:
                    {
                        if (!MethodParameterPacks.ContainsKey(parameterPackSourceCode.MethodSymbol))
                        {
                            MethodParameterPacks.Add(parameterPackSourceCode.MethodSymbol, parameterPackSourceCode);
                            return true;
                        }
                    }
                    break;

                case ResultPackSourceCode resultPackSourceCode:
                    {
                        if (!MethodResultPacks.ContainsKey(resultPackSourceCode.MethodSymbol))
                        {
                            MethodResultPacks.Add(resultPackSourceCode.MethodSymbol, resultPackSourceCode);
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

        public bool TryAddRealObjectInvokerSourceCode(INamedTypeSymbol realObjectTypeSymbol, INamedTypeSymbol interfaceTypeSymbol, RealObjectInvokerSourceCode sourceCode)
        {
            if (TypeRealObjectInvokers.TryGetValue(realObjectTypeSymbol, out var interfaceInvokerSourceCodes))
            {
                interfaceInvokerSourceCodes.Add(interfaceTypeSymbol, sourceCode);
            }
            else
            {
                interfaceInvokerSourceCodes = new(SymbolEqualityComparer.Default);
                interfaceInvokerSourceCodes.Add(interfaceTypeSymbol, sourceCode);

                TypeRealObjectInvokers.Add(realObjectTypeSymbol, interfaceInvokerSourceCodes);
            }
            return true;
        }

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