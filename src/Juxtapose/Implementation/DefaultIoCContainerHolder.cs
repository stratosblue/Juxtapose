namespace Juxtapose;

/// <summary>
/// 默认的 <inheritdoc cref="IIoCContainerHolder"/>
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="disposeServiceProvider">是否释放 <paramref name="serviceProvider"/></param>
public sealed class DefaultIoCContainerHolder(IServiceProvider serviceProvider, bool disposeServiceProvider)
    : IIoCContainerHolder
{
    #region Public 属性

    /// <inheritdoc/>
    public IServiceProvider ServiceProvider { get; } = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (!disposeServiceProvider)
        {
            return ValueTask.CompletedTask;
        }
        if (ServiceProvider is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }
        else if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        return ValueTask.CompletedTask;
    }

    #endregion Public 方法
}
