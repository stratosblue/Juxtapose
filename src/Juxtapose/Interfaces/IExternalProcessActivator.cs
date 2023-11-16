namespace Juxtapose;

/// <summary>
/// 外部进程激活器
/// </summary>
public interface IExternalProcessActivator : IDisposable
{
    #region Public 方法

    /// <summary>
    /// 创建<inheritdoc cref="IExternalProcess"/>
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    IExternalProcess CreateProcess(IJuxtaposeOptions options);

    #endregion Public 方法
}
