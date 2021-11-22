namespace Juxtapose
{
    /// <summary>
    /// <inheritdoc cref="IJuxtaposeExecutorPool"/> 策略
    /// </summary>
    public interface IExecutorPoolPolicy
    {
        #region Public 方法

        /// <summary>
        /// 区分上下文对应的执行器ID
        /// </summary>
        /// <param name="creationContext"></param>
        /// <param name="holdLimit">同时持有限制，null时为不限制</param>
        /// <returns></returns>
        string Classify(ExecutorCreationContext creationContext, out int? holdLimit);

        /// <summary>
        /// 是否应该抛弃执行器
        /// </summary>
        /// <param name="creationContext"></param>
        /// <param name="executorHolder"></param>
        /// <returns></returns>
        bool ShouldDropExecutor(ExecutorCreationContext creationContext, IJuxtaposeExecutorHolder executorHolder);

        #endregion Public 方法
    }
}