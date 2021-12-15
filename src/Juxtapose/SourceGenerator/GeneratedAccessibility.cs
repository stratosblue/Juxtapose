namespace Juxtapose.SourceGenerator
{
    /// <summary>
    /// 可访问性
    /// </summary>
    public enum GeneratedAccessibility
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
        /// 继承自基类型
        /// </summary>
        InheritBase,

        /// <summary>
        /// 继承自Context
        /// </summary>
        InheritContext,

        /// <summary>
        /// public
        /// </summary>
        Public,

        /// <summary>
        /// internal
        /// </summary>
        Internal,
    }
}