namespace Juxtapose;

/// <summary>
/// 引用ID
/// </summary>
public readonly record struct ReferenceId(int Id) : IEquatable<int>
{
    #region Public 方法

    /// <summary>
    /// 隐式类型转换
    /// <para/>
    /// 由类型 <see cref="ReferenceId"/> 对象转换到类型 <see cref="int"/> 对象
    /// </summary>
    /// <param name="value">源对象</param>
    public static implicit operator int(ReferenceId value)
    {
        return value.Id;
    }

    /// <summary>
    /// 隐式类型转换
    /// <para/>
    /// 由类型 <see cref="int"/> 对象转换到类型 <see cref="ReferenceId"/> 对象
    /// </summary>
    /// <param name="value">源对象</param>
    public static implicit operator ReferenceId(int value)
    {
        return new(value);
    }

    /// <summary>
    /// 运算重载
    /// </summary>
    public static bool operator !=(int a, ReferenceId b) => a != b.Id;

    /// <summary>
    /// 运算重载
    /// </summary>
    public static bool operator !=(ReferenceId a, int b) => a.Id != b;

    /// <summary>
    /// 运算重载
    /// </summary>
    public static bool operator ==(int a, ReferenceId b) => a == b.Id;

    /// <summary>
    /// 运算重载
    /// </summary>
    public static bool operator ==(ReferenceId a, int b) => a.Id == b;

    /// <inheritdoc/>
    public readonly bool Equals(int other)
    {
        return Id.Equals(other);
    }

    #endregion Public 方法
}
