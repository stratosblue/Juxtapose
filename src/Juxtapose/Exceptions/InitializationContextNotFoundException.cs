namespace Juxtapose
{
    /// <summary>
    /// 上下文未找到异常
    /// </summary>
    public class InitializationContextNotFoundException : JuxtaposeException
    {
        #region Public 构造函数

        /// <inheritdoc cref="InitializationContextNotFoundException"/>
        public InitializationContextNotFoundException(string identifier) : base($"context - {identifier} not found.")
        {
        }

        #endregion Public 构造函数
    }
}