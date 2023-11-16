using System.Text;

namespace Juxtapose;

/// <summary>
///
/// </summary>
public static class ISettingCollectionExtensions
{
    #region Public 方法

    /// <summary>
    /// 序列化为字符串
    /// </summary>
    /// <returns></returns>
    public static string Serialize(this ISettingCollection options)
    {
        var builder = new StringBuilder(512);
        foreach (var item in options)
        {
            builder.Append(item.Key);
            builder.Append('\t');
            builder.Append(item.Value);
            builder.Append('\n');
        }
        return builder.ToString();
    }

    #endregion Public 方法
}
