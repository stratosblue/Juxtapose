namespace Juxtapose.SourceGenerator.Model;

/// <summary>
/// 源代码
/// </summary>
public abstract class SourceCode
{
    #region Public 属性

    public string HintName { get; }

    public string Source { get; }

    #endregion Public 属性

    #region Public 构造函数

    public SourceCode(string hintName, string source)
    {
        if (string.IsNullOrWhiteSpace(hintName))
        {
            throw new System.ArgumentException($"“{nameof(hintName)}”不能为 null 或空白。", nameof(hintName));
        }

        if (string.IsNullOrWhiteSpace(source))
        {
            throw new System.ArgumentException($"“{nameof(source)}”不能为 null 或空白。", nameof(source));
        }

        HintName = hintName;
        Source = source;
    }

    #endregion Public 构造函数
}