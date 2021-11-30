using System;
using System.Threading;
using System.Threading.Tasks;

namespace Juxtapose
{
    /// <summary>
    /// 幻象对象池
    /// </summary>
    public interface IIllusionObjectPool<T>
        : IDisposable
        where T : IIllusion
    {
        #region Public 方法

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns>根据具体实现，可能获取到 null</returns>
        Task<T?> GetAsync(CancellationToken cancellation = default);

        /// <summary>
        /// 归还对象
        /// </summary>
        /// <param name="item">要归还的对象（允许传递 null ，实现时应当检查 null 并确保无事发生）</param>
        void Return(T? item);

        #endregion Public 方法
    }
}