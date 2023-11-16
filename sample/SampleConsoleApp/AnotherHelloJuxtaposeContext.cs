using Juxtapose;
using Juxtapose.SourceGenerator;

using SampleLibrary;

namespace SampleConsoleApp
{
    [Illusion(typeof(Hello), "AnotherHelloAsIHelloIllusion")]
    public partial class AnotherHelloJuxtaposeContext : JuxtaposeContext
    {
    }
}

namespace SampleLibrary
{
    public sealed partial class AnotherHelloAsIHelloIllusion : IHello
    { }
}
