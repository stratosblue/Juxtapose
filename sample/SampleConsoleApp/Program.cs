using Juxtapose;

using SampleConsoleApp;

using SampleLibrary;

await JuxtaposeEntryPoint.TryAsEndpointAsync(args, () => new IInitializationContext[] { HelloJuxtaposeContext.SharedInstance, AnotherHelloJuxtaposeContext.SharedInstance });

Console.WriteLine($"Current Process Id: {Environment.ProcessId}.");
{
    Console.WriteLine(" ----------------- HelloAsIHelloIllusion ----------------- ");
    using var instance = new HelloAsIHelloIllusion();

    Console.WriteLine(instance.Where());
    Console.WriteLine(await instance.SayAsync("Tom"));
}

{
    Console.WriteLine(" ----------------- AnotherHelloAsIHelloIllusion ----------------- ");
    using var anotherInstance = new AnotherHelloAsIHelloIllusion();

    Console.WriteLine(anotherInstance.Where());
    Console.WriteLine(await anotherInstance.SayAsync("Jerry"));
}

Console.WriteLine("Completed.");