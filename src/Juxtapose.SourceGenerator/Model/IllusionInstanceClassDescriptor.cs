using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

public class IllusionInstanceClassDescriptor : IllusionClassDescriptor
{
    #region Public 属性

    /// <summary>
    /// 从IoC容器中创建对象
    /// </summary>
    public bool FromIoCContainer { get; set; }

    #endregion Public 属性

    #region Public 构造函数

    public IllusionInstanceClassDescriptor(IllusionAttributeDefine attributeDefine, INamedTypeSymbol contextType) : base(attributeDefine, contextType)
    {
        if (TargetType.IsStatic)
        {
            throw new ArgumentException($"{TargetType} not a instance type");
        }

        FromIoCContainer = attributeDefine.FromIoCContainer;
    }

    #endregion Public 构造函数

    #region Public 方法

    public override bool Equals(IllusionClassDescriptor descriptor)
    {
        return descriptor is IllusionInstanceClassDescriptor instanceClassDescriptor
               && base.Equals(descriptor);
    }

    #endregion Public 方法
}
