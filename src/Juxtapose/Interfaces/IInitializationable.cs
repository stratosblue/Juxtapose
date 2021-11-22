using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose
{
    /// <summary>
    /// 可初始化的
    /// </summary>
    public interface IInitializationable
    {
        #region Public 方法

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        Task InitializationAsync(CancellationToken initializationToken);

        #endregion Public 方法
    }
}