using Juxtapose;

using SampleConsoleApp;

using SampleLibrary;

await JuxtaposeEntryPoint.TryAsEndpointAsync(args, () => new IInitializationContext[] { HelloJuxtaposeContext.SharedInstance, AnotherHelloJuxtaposeContext.SharedInstance });

Console.WriteLine($"Current Process Id: {Environment.ProcessId}.");
{
    Console.WriteLine(" ----------------- HelloIllusion ----------------- ");
    using var instance = new HelloIllusion();

    Console.WriteLine(instance.Where());
    Console.WriteLine(await instance.SayAsync("Tom"));
    Console.WriteLine((HelloOutputState)await instance.ProcessAsync(new HelloInputState("Tom", 123)));

    var ihello = instance as IHello;

    Console.WriteLine("IHello: " + ihello.Where());
    Console.WriteLine("IHello: " + await ihello.SayAsync("Tom"));
    Console.WriteLine("IHello: " + (HelloOutputState)await ihello.ProcessAsync(new HelloInputState("Tom", 123)));
}

{
    Console.WriteLine(" ----------------- AnotherHelloAsIHelloIllusion ----------------- ");
    using var anotherInstance = new AnotherHelloAsIHelloIllusion();

    Console.WriteLine(anotherInstance.Where());
    Console.WriteLine(await anotherInstance.SayAsync("Jerry"));
    Console.WriteLine((HelloOutputState)await anotherInstance.ProcessAsync(new HelloInputState("Jerry", 123)));

    var ihello = anotherInstance as IHello;

    Console.WriteLine("IHello: " + ihello.Where());
    Console.WriteLine("IHello: " + await ihello.SayAsync("Jerry"));
    Console.WriteLine("IHello: " + (HelloOutputState)await ihello.ProcessAsync(new HelloInputState("Jerry", 123)));
}

Console.WriteLine("Completed.");
