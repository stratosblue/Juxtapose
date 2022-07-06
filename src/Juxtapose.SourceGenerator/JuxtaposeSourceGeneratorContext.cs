using System.Collections.Generic;

using Juxtapose.SourceGenerator.Model;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator;

public class JuxtaposeSourceGeneratorContext
{
    #region Public 属性

    public GeneratorExecutionContext GeneratorExecutionContext { get; }

    public Dictionary<IllusionInstanceClassDescriptor, SubResourceCollection> IllusionInstanceClasses { get; private set; } = new();

    public Dictionary<IllusionStaticClassDescriptor, SubResourceCollection> IllusionStaticClasses { get; private set; } = new();

    public ContextResourceCollection Resources { get; private set; } = new();

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
    }

    #endregion Public 方法
}