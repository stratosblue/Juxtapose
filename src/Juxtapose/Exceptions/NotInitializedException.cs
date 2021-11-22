namespace Juxtapose
{
    /// <summary>
    /// 未初始化异常
    /// </summary>
    public class NotInitializedException : JuxtaposeException
    {
        #region Public 属性

        /// <summary>
        ///
        /// </summary>
        public string TargetName { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="NotInitializedException"/>
        public NotInitializedException(string targetName) : base($"{targetName} Not Initialized.")
        {
            TargetName = targetName;
        }

        #endregion Public 构造函数
    }
}