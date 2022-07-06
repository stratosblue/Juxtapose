using System;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

public class IllusionAttributeDefine
{
    #region Public 属性

    public GeneratedAccessibility Accessibility { get; }

    public AttributeData AttributeData { get; }

    /// <summary>
    /// 从IoC容器中创建对象
    /// </summary>
    public bool FromIoCContainer { get; set; }

    public string? GeneratedTypeName { get; }

    public INamedTypeSymbol? InheritType { get; }

    public INamedTypeSymbol TargetType { get; }

    #endregion Public 属性

    #region Public 构造函数

    public IllusionAttributeDefine(AttributeData attributeData)
    {
        if (attributeData is null)
        {
            throw new ArgumentNullException(nameof(attributeData));
        }

        //参数索引参见 - Juxtapose.SourceGenerator.IllusionAttribute

        if (attributeData.ConstructorArguments[0].Value is not INamedTypeSymbol targetType)
        {
            throw new ArgumentException($"{TypeFullNames.Juxtapose.SourceGenerator.IllusionAttribute_NoGlobal}.targetType 必须为有效类型");
        }

        TargetType = targetType;
        InheritType = attributeData.ConstructorArguments[1].Value as INamedTypeSymbol;
        GeneratedTypeName = attributeData.ConstructorArguments[2].Value as string;
        Accessibility = (GeneratedAccessibility)attributeData.ConstructorArguments[3].Value!;
        FromIoCContainer = (bool)attributeData.ConstructorArguments[4].Value!;

        AttributeData = attributeData;

        CheckValid();
    }

    #endregion Public 构造函数

    #region Private 方法

    private void CheckValid()
    {
        if (TargetType.TypeKind != TypeKind.Class)
        {
            if (!FromIoCContainer
                || TargetType.TypeKind != TypeKind.Interface)
            {
                throw new ArgumentException($"{TypeFullNames.Juxtapose.SourceGenerator.IllusionAttribute_NoGlobal}.targetType 必须为有效类型");
            }
        }

        if (TargetType.IsStatic)
        {
            if (InheritType is not null)
            {
                throw new ArgumentException($"静态类型 {TargetType.ToDisplayString()} 不能继承类型 {InheritType.ToDisplayString()}");
            }
            return;
        }

        if (TargetType.IsAbstract
            && !FromIoCContainer)
        {
            throw new ArgumentException($"{TypeFullNames.Juxtapose.SourceGenerator.IllusionAttribute_NoGlobal}.targetType 不能为抽象类型");
        }

        if (InheritType is not null)
        {
            if (InheritType.IsStatic)
            {
                throw new ArgumentException($"静态类型 {InheritType.ToDisplayString()} 不能被继承。");
            }

            if (InheritType.IsSealed)
            {
                throw new ArgumentException($"封闭类型 {InheritType.ToDisplayString()} 不能被继承。");
            }

            if (InheritType.TypeKind == TypeKind.Interface)
            {
                if (!TargetType.AllInterfaces.Contains(InheritType))
                {
                    throw new ArgumentException($"{TargetType.ToDisplayString()} 没有继承接口或类 {InheritType.ToDisplayString()}");
                }
            }
            else if (InheritType.TypeKind == TypeKind.Class)
            {
                if (!TargetType.IsDerivedFrom(InheritType))
                {
                    throw new ArgumentException($"{TargetType.ToDisplayString()} 没有继承接口或类 {InheritType.ToDisplayString()}");
                }
            }
            else
            {
                throw new ArgumentException($"{TargetType.ToDisplayString()} 的继承类型 {InheritType.ToDisplayString()} 必须为接口或实例类型。");
            }
        }
    }

    #endregion Private 方法
}