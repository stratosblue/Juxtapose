using Microsoft.Extensions.Logging;

namespace Juxtapose;

/// <summary>
/// Juxtapose初始化上下文
/// </summary>
public interface IInitializationContext
{
    #region Public 属性

    /// <summary>
    /// 传输选项
    /// </summary>
    CommunicationOptions CommunicationOptions { get; }

    /// <summary>
    /// 唯一标识符
    /// </summary>
    string Identifier { get; }

    /// <summary>
    /// 整个上下文使用的<see cref="ILoggerFactory"/>
    /// </summary>
    ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// 选项
    /// </summary>
    IReadOnlySettingCollection Options { get; }

    /// <summary>
    /// Juxtapose版本号
    /// </summary>
    int Version { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 创建<see cref="JuxtaposeExecutor"/>
    /// </summary>
    /// <param name="messageExchanger"></param>
    /// <returns></returns>
    JuxtaposeExecutor CreateExecutor(IMessageExchanger messageExchanger);

    /// <summary>
    /// 获取当前上下文的引导 <see cref="IJuxtaposeBootstrapper"/>
    /// </summary>
    /// <returns></returns>
    IJuxtaposeBootstrapper GetBootstrapper();

    /// <summary>
    /// 获取当前上下文的执行器池 <see cref="IJuxtaposeExecutorPool"/>
    /// </summary>
    /// <returns></returns>
    IJuxtaposeExecutorPool GetExecutorPool();

    #endregion Public 方法
}
