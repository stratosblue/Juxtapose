using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator.Model;

public class IllusionStaticClassDescriptor : IllusionClassDescriptor
{
    #region Public 构造函数

    public IllusionStaticClassDescriptor(IllusionAttributeDefine attributeDefine, INamedTypeSymbol contextType) : base(attributeDefine, contextType)
    {
    }

    #endregion Public 构造函数
}