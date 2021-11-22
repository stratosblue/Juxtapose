namespace Juxtapose.Messages
{
    /// <summary>
    /// 静态方法调用消息
    /// </summary>
    public class StaticMethodInvokeMessage<TParameterPack>
        : MethodInvokeMessage<TParameterPack>
        where TParameterPack : class
    {
        #region Public 方法

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"【StaticMethodInvokeMessage】Id: {Id}";
        }

        #endregion Public 方法
    }
}