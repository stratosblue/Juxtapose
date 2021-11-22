using Juxtapose;
using Juxtapose.SourceGenerator;

using SampleLibrary;

namespace SampleConsoleApp
{
    [IllusionClass(typeof(IHello), typeof(Hello), "AnotherHelloAsIHelloIllusion")]
    public partial class AnotherHelloJuxtaposeContext : JuxtaposeContext
    {
    }
}