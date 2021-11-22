using System.Security.Cryptography;
using System.Text;

namespace System
{
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

        #endregion Public 方法
    }
}