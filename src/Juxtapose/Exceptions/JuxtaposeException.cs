using System;

namespace Juxtapose
{
    /// <summary>
    /// Juxtapose异常
    /// </summary>
    public class JuxtaposeException : Exception
    {
        #region Public 构造函数

        /// <inheritdoc cref="JuxtaposeException"/>
        public JuxtaposeException()
        {
        }

        /// <inheritdoc cref="JuxtaposeException"/>
        public JuxtaposeException(string? message) : base(message)
        {
        }

        /// <inheritdoc cref="JuxtaposeException"/>
        public JuxtaposeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        #endregion Public 构造函数
    }
}