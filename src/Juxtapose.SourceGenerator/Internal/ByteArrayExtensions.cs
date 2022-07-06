using System.Text;

namespace System;

internal static class ByteArrayExtensions
{
    #region Public 方法

    public static string ToHexString(this byte[] data)
    {
        var builder = new StringBuilder(data.Length * 2);
        foreach (var item in data)
        {
            builder.AppendFormat("{0:x2}", item);
        }
        return builder.ToString();
    }

    #endregion Public 方法
}