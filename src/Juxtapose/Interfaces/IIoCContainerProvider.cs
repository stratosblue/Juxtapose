using System;
using System.Threading.Tasks;

namespace Juxtapose
{
    /// <summary>
    /// IoC容器提供器
    /// </summary>
    public interface IIoCContainerProvider
    {
        #region Public 方法

        /// <summary>
        /// 获取用于构建对象的Ioc容器 - <see cref="IServiceProvider"/>
        /// </summary>
        /// <returns>返回的 <see cref="IIoCContainerHolder"/> 将在 <see cref="JuxtaposeExecutor"/> 释放时释放</returns>
        ValueTask<IIoCContainerHolder> GetIoCContainerAsync();

        #endregion Public 方法
    }
}