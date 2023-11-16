namespace SampleLibrary;

public class Hello : IHello
{
    #region Public 方法

    /// <inheritdoc/>
    public async Task<string> SayAsync(string name)
    {
        await Task.CompletedTask;
        return $"Hello {name}.";
    }

    /// <inheritdoc/>
    public string Where()
    {
        return $"There is Process: {Environment.ProcessId}.";
    }

    #endregion Public 方法
}
