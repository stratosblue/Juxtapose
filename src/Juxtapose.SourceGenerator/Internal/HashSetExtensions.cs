namespace System.Collections.Generic;

internal static class HashSetExtensions
{
    #region Public 方法

    /// <summary>
    /// 添加并返回新加入集合的项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="set"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            if (set.Add(item))
            {
                yield return item;
            }
        }
    }

    #endregion Public 方法
}
