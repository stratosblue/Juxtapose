namespace Juxtapose.SourceGenerator
{
    /// <summary>
    /// 可访问性
    /// </summary>
    public enum IllusionClassAccessibility
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default,

        /// <summary>
        /// 继承自实现类
        /// </summary>
        InheritImplement = Default,

        /// <summary>
        /// 继承自接口
        /// </summary>
        InheritInterface,

        /// <summary>
        /// 继承自Context
        /// </summary>
        InheritContext,
    }
}