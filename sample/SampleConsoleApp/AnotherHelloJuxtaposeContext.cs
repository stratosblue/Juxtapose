using Juxtapose;
using Juxtapose.SourceGenerator;

using SampleLibrary;

namespace SampleConsoleApp
{
    [Illusion(typeof(Hello), typeof(IHello), "AnotherHelloAsIHelloIllusion")]
    public partial class AnotherHelloJuxtaposeContext : JuxtaposeContext
    {
    }
}