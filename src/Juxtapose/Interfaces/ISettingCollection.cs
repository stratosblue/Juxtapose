using System.Diagnostics.CodeAnalysis;

namespace Juxtapose;

/// <summary>
/// 只读的设置集合
/// </summary>
public interface IReadOnlySettingCollection : IEnumerable<KeyValuePair<string, string?>>
{
    #region Public 索引器

    /// <summary>
    /// 获取或设置 <paramref name="key"/> 的值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string? this[string key] { get; set; }

    #endregion Public 索引器

    #region Public 方法

    /// <summary>
    /// 尝试获取一个选项的值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool TryGetValue(string key, [NotNullWhen(true)] out string? value);

    #endregion Public 方法
}

/// <summary>
/// 设置集合
/// </summary>
public interface ISettingCollection : IEnumerable<KeyValuePair<string, string?>>
{
    #region Public 索引器

    /// <summary>
    /// 获取或设置 <paramref name="key"/> 的值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string? this[string key] { get; set; }

    #endregion Public 索引器

    #region Public 方法

    /// <summary>
    /// 尝试获取一个选项的值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool TryGetValue(string key, [NotNullWhen(true)] out string? value);

    #endregion Public 方法
}
