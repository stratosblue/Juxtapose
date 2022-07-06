namespace Juxtapose;

/// <summary>
/// 通道工厂
/// </summary>
public interface ICommunicationChannelFactory
{
    #region Public 方法

    /// <summary>
    /// 创建客户端
    /// </summary>
    /// <returns></returns>
    ICommunicationClient CreateClient(IJuxtaposeOptions options);

    /// <summary>
    /// 创建服务端
    /// </summary>
    /// <returns></returns>
    ICommunicationServer CreateServer(IJuxtaposeOptions options);

    #endregion Public 方法
}