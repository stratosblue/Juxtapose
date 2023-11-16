namespace Juxtapose;

/// <summary>
/// 外部进程
/// </summary>
public interface IExternalProcess : IInitializationable, IDisposable
{
    #region Public 事件

    /// <summary>
    /// 进程无效事件
    /// </summary>
    event Action<IExternalProcess>? OnProcessInvalid;

    #endregion Public 事件

    #region Public 属性

    /// <summary>
    /// 退出码
    /// </summary>
    int ExitCode { get; }

    /// <summary>
    /// 是否已退出
    /// </summary>
    bool HasExited { get; }

    /// <summary>
    /// 进程ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// 是否存活
    /// </summary>
    bool IsAlive { get; }

    /// <summary>
    /// 启动时间
    /// </summary>
    DateTime StartTime { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 获取内存使用量
    /// </summary>
    long? GetMemoryUsage();

    /// <summary>
    /// 获取标准错误输出
    /// </summary>
    /// <returns></returns>
    StreamReader? GetStandardError();

    /// <summary>
    /// 获取标准输出
    /// </summary>
    /// <returns></returns>
    StreamReader? GetStandardOutput();

    #endregion Public 方法
}
