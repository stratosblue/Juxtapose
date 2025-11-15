namespace Juxtapose;

/// <summary>
/// 已检查Id的<see cref="IUniqueIdGenerator"/>
/// </summary>
public class CheckedIdGenerator(Func<int, bool> checkFunc) : IUniqueIdGenerator
{
    #region Private 字段

    private readonly Func<int, bool> _checkFunc = checkFunc ?? throw new ArgumentNullException(nameof(checkFunc));

    private readonly IncreasingIdGenerator _increasingIdGenerator = new();

    #endregion Private 字段

    #region Public 方法

    /// <inheritdoc/>
    public int Next()
    {
        var id = _increasingIdGenerator.Next();
        while (_checkFunc(id))
        {
            id = _increasingIdGenerator.Next();
        }
        return id;
    }

    #endregion Public 方法
}
