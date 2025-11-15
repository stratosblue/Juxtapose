using System.Collections.Frozen;
using System.Diagnostics;

namespace Juxtapose;

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

        private readonly FrozenDictionary<string, IInitializationContext> _contexts;

        #endregion Private 字段

        #region Public 构造函数

        [DebuggerStepThrough]
        public ConstantExecutionContextLoader(params IInitializationContext[] contexts)
        {
            ArgumentNullException.ThrowIfNull(contexts);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(contexts.Length);

            _contexts = contexts.ToFrozenDictionary(static (m) => m.Identifier, static (m) => m);
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

    [method: DebuggerStepThrough]
    private class DelayExecutionContextLoader(Func<IEnumerable<IInitializationContext>> contextLoadAction)
        : IInitializationContextLoader
    {
        #region Private 字段

        private readonly Func<IEnumerable<IInitializationContext>> _contextLoadAction = contextLoadAction ?? throw new ArgumentNullException(nameof(contextLoadAction));

        private IReadOnlyDictionary<string, IInitializationContext>? _contexts;

        private IReadOnlyDictionary<string, IInitializationContext> Contexts => _contexts ??= LoadContexts();

        #endregion Private 字段

        #region Private 方法

        private FrozenDictionary<string, IInitializationContext> LoadContexts()
        {
            var contexts = _contextLoadAction()?.ToFrozenDictionary(static (m) => m.Identifier, static (m) => m)
                           ?? throw new InvalidOperationException("Context load faild");
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(contexts.Count);

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
