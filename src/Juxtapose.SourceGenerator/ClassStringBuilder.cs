using System;

namespace Juxtapose.SourceGenerator;

public class ClassStringBuilder : IndentStringBuilder
{
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
    public void Namespace(Action action, string name)
    {
        AppendIndentLine($"namespace {name}");
        Scope(action);
    }
}
