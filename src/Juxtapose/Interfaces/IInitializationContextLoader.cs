namespace Juxtapose
{
    /// <summary>
    /// <inheritdoc cref="IInitializationContext"/>加载器
    /// </summary>
    public interface IInitializationContextLoader
    {
        #region Public 方法

        /// <summary>
        /// 检查是否包含<paramref name="identifier"/>对应的初始化上下文
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        bool Contains(string identifier);

        /// <summary>
        /// 获取<paramref name="identifier"/>对应的<inheritdoc cref="IInitializationContext"/>
        /// </summary>
        /// <returns></returns>
        IInitializationContext Get(string identifier);

        #endregion Public 方法
    }
}