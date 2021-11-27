using Juxtapose;
using Juxtapose.SourceGenerator;

namespace SampleLibrary
{
    [Illusion(typeof(Hello), typeof(IHello))]
    public partial class HelloJuxtaposeContext : JuxtaposeContext
    {
    }
}