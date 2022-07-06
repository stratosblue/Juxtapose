using System;

namespace Juxtapose;

/// <summary>
/// 外部工作者
/// </summary>
public interface IExternalWorker : IMessageExchanger, IInitializationable, IDisposable
{
    #region Public 属性

    /// <summary>
    /// 绑定的外部进程
    /// </summary>
    IExternalProcess ExternalProcess { get; }

    #endregion Public 属性
}