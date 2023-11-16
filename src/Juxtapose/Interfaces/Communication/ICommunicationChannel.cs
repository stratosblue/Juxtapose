namespace Juxtapose;

/// <summary>
/// 传输通道
/// </summary>
public interface ICommunicationChannel : IInitializationable, IDisposable
{
    #region Public 方法

    /// <summary>
    /// 注册初始化时的流建立回调
    /// </summary>
    /// <param name="onConnectedCallback"></param>
    /// <returns></returns>
    IDisposable RegisterOnInitializationConnected(Action<Stream> onConnectedCallback);

    #endregion Public 方法
}
