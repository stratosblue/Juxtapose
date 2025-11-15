namespace Juxtapose.Internal;

internal sealed class ActionDisposer(Action action) : IDisposable
{
    #region Private 字段

    private readonly Action _action = action ?? throw new ArgumentNullException(nameof(action));

    private int _isDisposed = 0;

    #endregion Private 字段

    #region Public 方法

    public void Dispose()
    {
        if (Interlocked.Increment(ref _isDisposed) == 1)
        {
            _action();
        }
    }

    #endregion Public 方法
}
