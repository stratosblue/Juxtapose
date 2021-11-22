using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Juxtapose
{
    /// <inheritdoc cref="IInitializationContextLoader"/>
    public static class InitializationContextLoader
    {
        #region Public 方法

        /// <summary>
        /// 从 <paramref name="contexts"/> 创建 <see cref="IInitializationContextLoader"/>
        /// </summary>
        /// <param name="contexts"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IInitializationContextLoader Create(params IInitializationContext[] contexts)
        {
            return new ConstantExecutionContextLoader(contexts);
        }

        /// <summary>
        /// 从 <paramref name="contextLoadAction"/> 创建 <see cref="IInitializationContextLoader"/>
        /// </summary>
        /// <param name="contextLoadAction"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IInitializationContextLoader Create(Func<IEnumerable<IInitializationContext>> contextLoadAction)
        {
            return new DelayExecutionContextLoader(contextLoadAction);
        }

        #endregion Public 方法

        #region Private 类

        private class ConstantExecutionContextLoader : IInitializationContextLoader
        {
            #region Private 字段

            private readonly IReadOnlyDictionary<string, IInitializationContext> _contexts;

            #endregion Private 字段

            #region Public 构造函数

            [DebuggerStepThrough]
            public ConstantExecutionContextLoader(params IInitializationContext[] contexts)
            {
                if (contexts is null
                    || contexts.Length is 0)
                {
                    throw new ArgumentException("invalid contexts", nameof(contexts));
                }

                _contexts = contexts?.ToDictionary(static (m) => m.Identifier, static (m) => m) ?? throw new ArgumentNullException(nameof(contexts));
            }

            #endregion Public 构造函数

            #region Public 方法

            /// <inheritdoc/>
            public bool Contains(string identifier) => _contexts.ContainsKey(identifier);

            /// <inheritdoc/>
            public IInitializationContext Get(string identifier)
            {
                if (_contexts.TryGetValue(identifier, out var context))
                {
                    return context;
                }
                throw new InitializationContextNotFoundException(identifier);
            }

            #endregion Public 方法
        }

        private class DelayExecutionContextLoader : IInitializationContextLoader
        {
            #region Private 字段

            private readonly Func<IEnumerable<IInitializationContext>> _contextLoadAction;

            private IReadOnlyDictionary<string, IInitializationContext>? _contexts;

            private IReadOnlyDictionary<string, IInitializationContext> Contexts => _contexts ??= LoadContexts();

            #endregion Private 字段

            #region Public 构造函数

            [DebuggerStepThrough]
            public DelayExecutionContextLoader(Func<IEnumerable<IInitializationContext>> contextLoadAction)
            {
                _contextLoadAction = contextLoadAction ?? throw new ArgumentNullException(nameof(contextLoadAction));
            }

            #endregion Public 构造函数

            #region Private 方法

            private IReadOnlyDictionary<string, IInitializationContext> LoadContexts()
            {
                var contexts = _contextLoadAction().ToDictionary(static (m) => m.Identifier, static (m) => m) ?? throw new ArgumentNullException(nameof(_contextLoadAction));
                if (contexts.Count is 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(contexts));
                }
                return contexts;
            }

            #endregion Private 方法

            #region Public 方法

            /// <inheritdoc/>
            public bool Contains(string identifier) => Contexts.ContainsKey(identifier);

            /// <inheritdoc/>
            public IInitializationContext Get(string identifier)
            {
                if (Contexts.TryGetValue(identifier, out var context))
                {
                    return context;
                }
                throw new InitializationContextNotFoundException(identifier);
            }

            #endregion Public 方法
        }

        #endregion Private 类
    }
}