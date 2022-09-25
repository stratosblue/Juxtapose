using System;
using System.Linq;
using System.Text;

namespace Juxtapose.SourceGenerator;

public class IndentStringBuilder
{
    #region Public 字段

    /// <summary>
    /// 预定义缩进列表
    /// </summary>
    public static readonly string[] Indents = Enumerable.Range(0, 128).Select(i => new string(Enumerable.Repeat(' ', i * 4).ToArray())).ToArray();

    #endregion Public 字段

    #region Protected 属性

    /// <summary>
    /// 当前缩进大小
    /// </summary>
    protected ushort CurrentIndent { get; set; } = 0;

    #endregion Protected 属性

    #region Public 属性

    /// <summary>
    /// 使用的<see cref="StringBuilder"/>
    /// </summary>
    public StringBuilder Builder { get; }

    #endregion Public 属性

    #region Public 构造函数

    public IndentStringBuilder(int capacity = 4096)
    {
        Builder = new(capacity);
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 添加<paramref name="value"/>，不换行不缩进
    /// </summary>
    /// <param name="value"></param>
    public void Append(string value)
    {
        Builder.Append(value);
    }

    /// <summary>
    /// 添加<paramref name="value"/>为单行，并在其前面添加当前缩进(不换行)
    /// </summary>
    /// <param name="value"></param>
    public void AppendIndent(string value)
    {
        AppendIndentSpace();
        Builder.Append(value);
    }

    /// <summary>
    /// 添加<paramref name="value"/>为单行，并在其前面添加当前缩进
    /// </summary>
    /// <param name="value"></param>
    /// <param name="appendLine">额外追加空行</param>
    public void AppendIndentLine(string value, bool appendLine = false)
    {
        AppendIndentSpace();
        Builder.AppendLine(value);

        if (appendLine)
        {
            Builder.AppendLine();
        }
    }

    /// <summary>
    /// 添加当前缩进的空格（不换行）
    /// </summary>
    public void AppendIndentSpace()
    {
        Builder.Append(Indents[CurrentIndent]);
    }

    /// <inheritdoc cref="StringBuilder.AppendLine"/>
    public void AppendLine()
    {
        Builder.AppendLine();
    }

    /// <summary>
    /// 拆分<paramref name="value"/>为每行，为每行附加缩进
    /// </summary>
    /// <param name="value"></param>
    public void AppendLine(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }
        var lines = value!.Trim().Replace("\r\n", "\n").Split('\n');
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                AppendIndentLine(line);
            }
            else
            {
                AppendLine();
            }
        }
    }

    /// <summary>
    /// 添加作用域，拆分<paramref name="value"/>为每行，为每行附加缩进
    /// <para/>
    /// {<para/>
    ///     <paramref name="value"/><para/>
    /// }
    /// </summary>
    /// <param name="value"></param>
    public void AppendScope(string value)
    {
        AppendIndentLine("{");
        Indent();
        AppendLine(value);
        Dedent();
        AppendIndentLine("}");
    }

    public void Clear()
    {
        Builder.Clear();
    }

    /// <summary>
    /// 减少缩进量
    /// </summary>
    public void Dedent()
    {
        if (CurrentIndent > 0)
        {
            CurrentIndent--;
        }
    }

    /// <summary>
    /// 增加缩进量
    /// </summary>
    public void Indent()
    {
        CurrentIndent++;
    }

    /// <summary>
    /// 在一个作用域内执行委托<paramref name="action"/>
    /// <para/>
    /// {<para/>
    ///     <paramref name="action"/><para/>
    /// }
    /// </summary>
    /// <param name="action"></param>
    public void Scope(Action action)
    {
        AppendIndentLine("{");
        Indent();
        action();
        Dedent();
        AppendIndentLine("}");
    }

    /// <inheritdoc/>
    public override string ToString() => Builder.ToString();

    #endregion Public 方法
}