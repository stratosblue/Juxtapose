using System.Text.Json.Serialization;

namespace SampleLibrary;

public interface IHello
{
    #region Public 方法

    /// <summary>
    /// process
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    Task<HelloState> ProcessAsync(HelloState state);

    /// <summary>
    /// say
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<string> SayAsync(string name);

    /// <summary>
    /// where
    /// </summary>
    /// <returns></returns>
    string Where();

    #endregion Public 方法
}

[JsonDerivedType(typeof(HelloInputState), typeDiscriminator: "input")]
[JsonDerivedType(typeof(HelloOutputState), typeDiscriminator: "output")]
public record class HelloState(string Name);

public record class HelloInputState(string Name, int Input) : HelloState(Name);

public record class HelloOutputState(string Name, int Input, string Output) : HelloState(Name);
