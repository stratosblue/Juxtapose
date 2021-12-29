namespace ResourceBasedDynamicObjectPool
{
    public class ResourceBasedObjectPoolSchedulerOptions
    {
        #region Public 属性

        /// <summary>
        /// 自动缩减池的间隔
        /// </summary>
        public TimeSpan AutoContractionInterval { get; set; } = TimeSpan.FromMinutes(0.75);

        /// <summary>
        /// 缩减池时，对象闲置超过此时间将会销毁
        /// </summary>
        public TimeSpan DestoryIdleObjectTime { get; set; } = TimeSpan.FromMinutes(0.75);

        /// <summary>
        /// 进程数量基线
        /// </summary>
        public int ProcessCountBaseLine { get; set; } = Environment.ProcessorCount > 1 ? (int)(Environment.ProcessorCount * 1.75) : Environment.ProcessorCount * 2;

        #endregion Public 属性
    }
}