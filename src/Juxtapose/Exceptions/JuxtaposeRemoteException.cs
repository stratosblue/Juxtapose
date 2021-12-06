using System;
using System.Runtime.Serialization;

namespace Juxtapose
{
    /// <summary>
    /// Juxtapose远程异常
    /// </summary>
    [Serializable]
    public class JuxtaposeRemoteException : JuxtaposeException
    {
        #region Private 字段

        private readonly string _originToStringValue;

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 原始异常类型
        /// </summary>
        public string OriginExceptionType { get; }

        /// <summary>
        /// 异常的原始<see cref="Exception.Message"/>
        /// </summary>
        public string OriginMessage { get; }

        /// <summary>
        /// 异常的原始<see cref="Exception.StackTrace"/>
        /// </summary>
        public string? OriginStackTrace { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="JuxtaposeRemoteException"/>
        /// </summary>
        /// <param name="originStackTrace"></param>
        /// <param name="originMessage"></param>
        /// <param name="originToStringValue"></param>
        /// <param name="originExceptionType"></param>
        /// <param name="message"></param>
        public JuxtaposeRemoteException(string? originStackTrace, string originMessage, string originToStringValue, string originExceptionType, string? message = null)
            : base(message ?? $"Exception [{originExceptionType}] has threw at remote.")
        {
            OriginStackTrace = originStackTrace;
            OriginMessage = originMessage;
            _originToStringValue = originToStringValue;
            OriginExceptionType = originExceptionType;
        }

        #endregion Public 构造函数

        #region Protected 构造函数

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        /// <inheritdoc cref="JuxtaposeRemoteException"/>
        protected JuxtaposeRemoteException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        #endregion Protected 构造函数

        #region Public 方法

        /// <summary>
        /// 原始<see cref="Exception.ToString()"/>
        /// </summary>
        /// <returns></returns>
        public string OriginToString() => _originToStringValue;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $@"{base.ToString()}
┌------------ Remote Exception ------------┐
{_originToStringValue}
└------------ Remote Exception ------------┘";
        }

        #endregion Public 方法
    }
}