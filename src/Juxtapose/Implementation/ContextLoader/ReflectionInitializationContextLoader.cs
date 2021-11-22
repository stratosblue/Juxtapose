using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Juxtapose
{
    /// <summary>
    /// 基于反射的自动<inheritdoc cref="IInitializationContextLoader"/>
    /// </summary>
    public class ReflectionInitializationContextLoader : IInitializationContextLoader
    {
        #region Private 字段

        private readonly Dictionary<string, IInitializationContext> _contexts;

        #endregion Private 字段

        #region Public 构造函数

        /// <inheritdoc cref="ReflectionInitializationContextLoader"/>
        public ReflectionInitializationContextLoader()
        {
            _contexts = AppDomain.CurrentDomain.GetAssemblies()
                                               .SelectMany(GetAssemblyTypes)
                                               .Where(IsContext)
                                               .Select(TryGetContextInstance)
                                               .OfType<IInitializationContext>()
                                               .ToDictionary(m => m.Identifier, m => m);

            static IEnumerable<Type> GetAssemblyTypes(Assembly assembly)
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            }

            static bool IsContext(Type type)
            {
                return !type.IsInterface
                       && !type.IsAbstract
                       && type.IsAssignableTo(typeof(IInitializationContext));
            }

            static IInitializationContext? TryGetContextInstance(Type type)
            {
                if (type.GetProperty("SharedInstance", BindingFlags.Public | BindingFlags.Static) is PropertyInfo propertyInfo
                    && propertyInfo.CanRead
                    && IsContext(propertyInfo.PropertyType))
                {
                    return propertyInfo.GetValue(null, null) as IInitializationContext;
                }
                else if (type.GetConstructor(Type.EmptyTypes) is ConstructorInfo constructorInfo
                         && constructorInfo.IsPublic)
                {
                    return Activator.CreateInstance(type) as IInitializationContext;
                }
                return null;
            }
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
}