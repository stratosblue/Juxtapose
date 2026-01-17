namespace Juxtapose;

/// <summary>
/// 外部进程持有者
/// </summary>
public interface IExternalProcessHolder
{
    #region Public 属性

    /// <summary>
    /// 持有的外部进程
    /// </summary>
    public IExternalProcess? ExternalProcess { get; }

    #endregion Public 属性
}
