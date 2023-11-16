namespace Juxtapose;

/// <summary>
/// 空的 <inheritdoc cref="IIoCContainerHolder"/>
/// </summary>
public sealed class EmptyIoCContainerHolder : IIoCContainerHolder
{
    #region Public 属性

    /// <summary>
    /// 共享实例
    /// </summary>
    public static EmptyIoCContainerHolder Instance { get; } = new();

    /// <inheritdoc/>
    public IServiceProvider ServiceProvider { get; } = EmptyServiceProvider.Instance;

    #endregion Public 属性

    #region Private 构造函数

    private EmptyIoCContainerHolder()
    {
    }

    #endregion Private 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    #endregion Public 方法

    #region Private 类

    internal class EmptyServiceProvider : IServiceProvider
    {
        #region Public 字段

        public static readonly EmptyServiceProvider Instance = new();

        #endregion Public 字段

        #region Private 构造函数

        private EmptyServiceProvider()
        {
        }

        #endregion Private 构造函数

        #region Public 方法

        public object? GetService(Type serviceType)
        {
            throw new InvalidOperationException($"can not use {nameof(EmptyServiceProvider)} do any thing.");
        }

        #endregion Public 方法
    }

    #endregion Private 类
}
