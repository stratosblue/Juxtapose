using System;

namespace Juxtapose.SourceGenerator;

public class ClassStringBuilder : IndentStringBuilder
{
    #region Public 构造函数

    public ClassStringBuilder(int capacity = 4096) : base(capacity)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 在一个命名空间内执行委托<paramref name="action"/>
    /// <para/>
    /// namespace <paramref name="name"/><para/>
    /// {<para/>
    ///     <paramref name="action"/><para/>
    /// }
    /// </summary>
    /// <param name="action"></param>
    /// <param name="name"></param>
    public void Namespace(Action action, string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            AppendIndentLine($"namespace {name}");
            Scope(action);
        }
        else
        {
            action();
        }
    }

    #endregion Public 方法
}
