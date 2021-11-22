using System;
using System.Threading;

namespace Juxtapose
{
    /// <inheritdoc cref="IJuxtaposeExecutorOwner"/>
    public sealed class JuxtaposeExecutorOwner : IJuxtaposeExecutorOwner
    {
        #region Private 字段

        private readonly ExecutorCreationContext _creationContext;
        private readonly ExecutorHolderDestroyCallback _destroyCallback;
        private readonly IJuxtaposeExecutorHolder _executorHolder;
        private readonly IExecutorPoolPolicy _executorPoolPolicy;
        private readonly string _identifier;
        private int _isDisposed = 0;

        #endregion Private 字段

        #region Public 属性

        /// <inheritdoc/>
        public JuxtaposeExecutor Executor => _isDisposed > 0 ? throw new ObjectDisposedException(nameof(JuxtaposeExecutorOwner)) : _executorHolder.Executor;

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="JuxtaposeExecutorOwner"/>
        public JuxtaposeExecutorOwner(string identifier, IJuxtaposeExecutorHolder executorHolder, ExecutorCreationContext creationContext, IExecutorPoolPolicy executorPoolPolicy, ExecutorHolderDestroyCallback destroyCallback)
        {
            _identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            _executorHolder = executorHolder ?? throw new ArgumentNullException(nameof(executorHolder));
            _creationContext = creationContext ?? throw new ArgumentNullException(nameof(creationContext));
            _executorPoolPolicy = executorPoolPolicy ?? throw new ArgumentNullException(nameof(executorPoolPolicy));
            _destroyCallback = destroyCallback ?? throw new ArgumentNullException(nameof(destroyCallback));
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Increment(ref _isDisposed) == 1)
            {
                _executorHolder.Release();

                if (_executorPoolPolicy.ShouldDropExecutor(_creationContext, _executorHolder))
                {
                    _destroyCallback(_identifier);
                }
            }
        }

        #endregion Public 方法
    }
}