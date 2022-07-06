using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

public class TypeConstructorDescriptor
{
    #region Public 属性

    public IReadOnlyDictionary<string, ConstructorParameterPackSourceCode> ConstructorMaps { get; }

    public INamedTypeSymbol TargetType { get; }

    #endregion Public 属性

    #region Public 构造函数

    public TypeConstructorDescriptor(INamedTypeSymbol targetType, IReadOnlyDictionary<string, ConstructorParameterPackSourceCode> constructorMaps)
    {
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        ConstructorMaps = constructorMaps ?? throw new ArgumentNullException(nameof(constructorMaps));
    }

    #endregion Public 构造函数
}