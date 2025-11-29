namespace SampleLibrary;

public class Hello : IHello
{
    #region Public 方法

    public Task<HelloState> ProcessAsync(HelloState state)
    {
        var input = (HelloInputState)state;

        return Task.FromResult<HelloState>(new HelloOutputState(input.Name, input.Input, input.Input.ToString()));
    }

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
