﻿namespace Juxtapose;

/// <summary>
/// 常量
/// </summary>

#if JUXTAPOSE_SOURCE_GENERATOR

internal static class Constants

#else

public static class Constants

#endif
{
    #region Public 字段

    /// <summary>
    /// ID 阈值
    /// </summary>
    public const int IdThreshold = int.MaxValue - 0x0FFF_FFFF;

    /// <summary>
    /// <see cref="OperationCanceledException"/> 完整名称
    /// </summary>
    public const string OperationCanceledExceptionFullType = "System.OperationCanceledException";

    /// <summary>
    /// Juxtapose版本号
    /// </summary>
    public const uint Version = 2;

    #endregion Public 字段
}
