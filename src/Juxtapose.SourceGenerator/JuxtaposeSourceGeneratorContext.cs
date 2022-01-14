using System;
using System.Collections.Generic;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

#pragma warning disable RS1024 // 正确比较符号

namespace Juxtapose.SourceGenerator
{
    public class JuxtaposeSourceGeneratorContext
    {
        #region Public 属性

        public GeneratorExecutionContext GeneratorExecutionContext { get; }

        public Dictionary<IllusionInstanceClassDescriptor, SubResourceCollection> IllusionInstanceClasses { get; private set; } = new();

        public Dictionary<IllusionStaticClassDescriptor, SubResourceCollection> IllusionStaticClasses { get; private set; } = new();

        public ContextResourceCollection Resources { get; private set; } = new();

        /// <summary>
        /// 由 <see cref="IServiceProvider"/> 提供的类型集合
        /// </summary>
        public HashSet<INamedTypeSymbol> ServiceProviderProvideTypes { get; private set; } = new(SymbolEqualityComparer.Default);

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
            IllusionInstanceClasses = new();
            IllusionStaticClasses = new();
            Resources = new();
            ServiceProviderProvideTypes = new(SymbolEqualityComparer.Default);
        }

        #endregion Public 方法
    }
}