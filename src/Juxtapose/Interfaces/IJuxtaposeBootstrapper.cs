namespace Juxtapose;

/// <summary>
/// Juxtapose启用引导
/// </summary>
public interface IJuxtaposeBootstrapper : IDisposable
{
    #region Public 方法

    /// <summary>
    /// 准备一个可用的执行器
    /// </summary>
    /// <param name="options">初始化选项</param>
    /// <param name="initializationToken"></param>
    /// <returns></returns>
    Task<JuxtaposeExecutor> PrepareExecutorAsync(IJuxtaposeOptions options, CancellationToken initializationToken);

    /// <summary>
    /// 准备一个可用的主机端执行器
    /// </summary>
    /// <param name="initializationToken"></param>
    /// <returns></returns>
    Task<JuxtaposeExecutor> PrepareHostingExecutorAsync(CancellationToken initializationToken);

    #endregion Public 方法
}
