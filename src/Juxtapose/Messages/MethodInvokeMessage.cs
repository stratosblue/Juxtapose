namespace Juxtapose.Messages
{
    /// <summary>
    /// 方法调用消息
    /// </summary>
    /// <typeparam name="TParameterPack">方法参数包类型</typeparam>
    public abstract class MethodInvokeMessage<TParameterPack>
        : JuxtaposeMessage
        where TParameterPack : class
    {
        #region Public 属性

        /// <summary>
        /// 方法参数包
        /// </summary>
        public TParameterPack? ParameterPack { get; set; }

        #endregion Public 属性

        #region Public 方法

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"【{nameof(MethodInvokeMessage<TParameterPack>)}】Id: {Id}";
        }

        #endregion Public 方法
    }
}