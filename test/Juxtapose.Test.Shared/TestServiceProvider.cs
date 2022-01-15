using System;

using Microsoft.Extensions.DependencyInjection;

namespace Juxtapose.Test
{
    public static class TestServiceProvider
    {
        #region Public 方法

        /// <summary>
        /// 创建一个测试的IServiceProvider
        /// </summary>
        /// <returns></returns>
        public static IServiceProvider Create()
        {
            var services = new ServiceCollection();

            services.AddScoped<IGreeter, Greeter>();
            services.AddScoped<GreeterFromServiceProvider>();

            return services.BuildServiceProvider();
        }

        #endregion Public 方法
    }
}