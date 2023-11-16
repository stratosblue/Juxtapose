namespace Juxtapose.SourceGenerator.Model;

/// <summary>
/// 真实对象调用
/// </summary>
public class RealObjectInvokerSourceCode : FullSourceCode
{
    #region Public 属性

    public string TypeFullName { get; set; }

    public string TypeName { get; set; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="RealObjectInvokerSourceCode"/>
    public RealObjectInvokerSourceCode(string hintName, string source, string typeName, string typeFullName) : base(hintName, source)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException($"“{nameof(typeName)}”不能为 null 或空白。", nameof(typeName));
        }

        if (string.IsNullOrWhiteSpace(typeFullName))
        {
            throw new ArgumentException($"“{nameof(typeFullName)}”不能为 null 或空白。", nameof(typeFullName));
        }

        TypeName = typeName;
        TypeFullName = typeFullName;
    }

    #endregion Public 构造函数
}
