using System;

namespace Juxtapose.SourceGenerator.Model;

/// <summary>
/// 部分源代码
/// </summary>
public class PartialSourceCode : SourceCode
{
    #region Public 属性

    /// <summary>
    /// 包装类型的命名空间
    /// </summary>
    public string Namespace { get; }

    public string TypeFullName { get; }

    public string TypeName { get; }

    #endregion Public 属性

    #region Public 构造函数

    public PartialSourceCode(string hintName,
                             string source,
                             string @namespace,
                             string typeName,
                             string typeFullName) : base(hintName, source)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
        {
            throw new ArgumentException($"“{nameof(@namespace)}”不能为 null 或空白。", nameof(@namespace));
        }

        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException($"“{nameof(typeName)}”不能为 null 或空白。", nameof(typeName));
        }

        if (string.IsNullOrWhiteSpace(typeFullName))
        {
            throw new ArgumentException($"“{nameof(typeFullName)}”不能为 null 或空白。", nameof(typeFullName));
        }

        Namespace = @namespace;
        TypeName = typeName;
        TypeFullName = typeFullName;
    }

    #endregion Public 构造函数
}