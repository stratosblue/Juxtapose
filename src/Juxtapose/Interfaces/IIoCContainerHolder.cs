using System;

namespace Juxtapose;

/// <summary>
/// IoC容器持有器
/// </summary>
public interface IIoCContainerHolder : IAsyncDisposable
{
    #region Public 属性

    /// <inheritdoc cref="IServiceProvider"/>
    IServiceProvider ServiceProvider { get; }

    #endregion Public 属性
}