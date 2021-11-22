namespace Juxtapose
{
    /// <summary>
    /// 唯一ID生成器
    /// </summary>
    public interface IUniqueIdGenerator
    {
        #region Public 方法

        /// <summary>
        /// 获取下一个Id(正整数)
        /// </summary>
        int Next();

        #endregion Public 方法
    }
}