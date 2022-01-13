using System;
using System.Threading.Tasks;

namespace Juxtapose
{
    /// <summary>
    /// 默认的 <inheritdoc cref="IIoCContainerHolder"/>
    /// </summary>
    public sealed class DefaultIoCContainerHolder : IIoCContainerHolder
    {
        #region Private 字段

        private readonly bool _disposeServiceProvider;

        #endregion Private 字段

        #region Public 属性

        /// <inheritdoc/>
        public IServiceProvider ServiceProvider { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="DefaultIoCContainerHolder"/>
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="disposeServiceProvider">是否释放 <paramref name="serviceProvider"/></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultIoCContainerHolder(IServiceProvider serviceProvider, bool disposeServiceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _disposeServiceProvider = disposeServiceProvider;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            if (!_disposeServiceProvider)
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
}