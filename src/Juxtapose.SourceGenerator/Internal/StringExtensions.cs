using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace System;

internal static class StringExtensions
{
    #region Public 方法

    /// <summary>
    /// 计算md5
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string CalculateMd5(this string text)
    {
        using var md5 = MD5.Create();
        return md5.ComputeHash(Encoding.UTF8.GetBytes(text)).ToHexString();
    }

    public static string NormalizeAsClassName(this string text)
    {
        return Regex.Replace(text, "[^0-9a-zA-Z_]", "_");
    }

    #endregion Public 方法
}
