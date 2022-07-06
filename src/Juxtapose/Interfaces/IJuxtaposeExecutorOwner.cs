using System;

namespace Juxtapose;

/// <summary>
/// <see cref="JuxtaposeExecutor"/> 所有者
/// </summary>
public interface IJuxtaposeExecutorOwner : IDisposable
{
    #region Public 属性

    /// <inheritdoc cref="JuxtaposeExecutor"/>
    JuxtaposeExecutor Executor { get; }

    #endregion Public 属性
}