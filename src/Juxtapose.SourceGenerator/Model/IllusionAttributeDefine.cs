using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

[DebuggerDisplay("{Accessibility} {TargetType} {GeneratedTypeName} {FromIoCContainer}")]
public sealed class IllusionAttributeDefine
{
    #region Public 属性

    internal GeneratedAccessibility Accessibility { get; }

    public AttributeData AttributeData { get; }

    /// <summary>
    /// 从IoC容器中创建对象
    /// </summary>
    public bool FromIoCContainer { get; set; }

    public string? GeneratedTypeName { get; }

    [Obsolete("使用 partial 类来实现继承", true)]
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

        var arguments = attributeData.ConstructorArguments;

        if (arguments[0].Value is not INamedTypeSymbol targetType)
        {
            throw new ArgumentException($"{TypeFullNames.Juxtapose.SourceGenerator.IllusionAttribute_NoGlobal}.targetType 必须为有效类型");
        }

        TargetType = targetType;

        switch (arguments.Length)
        {
            case 4:
                GeneratedTypeName = arguments[1].Value as string;
                Accessibility = (GeneratedAccessibility)arguments[2].Value!;
                FromIoCContainer = (bool)arguments[3].Value!;
                break;

            case 5:
                GeneratedTypeName = arguments[2].Value as string;
                Accessibility = (GeneratedAccessibility)arguments[3].Value!;
                FromIoCContainer = (bool)arguments[4].Value!;
                break;
        }

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
            return;
        }

        if (TargetType.IsAbstract
            && !FromIoCContainer)
        {
            throw new ArgumentException($"{TypeFullNames.Juxtapose.SourceGenerator.IllusionAttribute_NoGlobal}.targetType 不能为抽象类型");
        }
    }

    #endregion Private 方法
}
