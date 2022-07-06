using System;
using System.IO.Pipes;
using System.Threading;

namespace Juxtapose.Communication.Channel;

/// <summary>
/// 基于 <see cref="NamedPipeClientStream"/> 和 <see cref="NamedPipeServerStream"/> 的 <inheritdoc cref="ICommunicationChannelFactory"/>
/// </summary>
public class NamedPipeCommunicationChannelFactory : ICommunicationChannelFactory
{
    #region static shared

    private static readonly Lazy<NamedPipeCommunicationChannelFactory> s_shared = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 共享的 <see cref="NamedPipeCommunicationChannelFactory"/> 实例
    /// </summary>
    public static NamedPipeCommunicationChannelFactory Shared => s_shared.Value;

    #endregion static shared

    #region Public 方法

    /// <inheritdoc/>
    public ICommunicationClient CreateClient(IJuxtaposeOptions options)
    {
        return new NamedPipeCommunicationClient(options.SessionId, ".");
    }

    /// <inheritdoc/>
    public ICommunicationServer CreateServer(IJuxtaposeOptions options)
    {
        return new NamedPipeCommunicationServer(options.SessionId);
    }

    #endregion Public 方法
}