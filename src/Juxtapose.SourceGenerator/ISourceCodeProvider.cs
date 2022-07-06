using System.Collections.Generic;
using Juxtapose.SourceGenerator.Model;

namespace Juxtapose.SourceGenerator;

public interface ISourceCodeProvider<out TSourceCode> where TSourceCode : SourceCode
{
    #region Public 方法

    /// <summary>
    /// 获取源代码
    /// </summary>
    /// <returns></returns>
    IEnumerable<TSourceCode> GetSources();

    #endregion Public 方法
}