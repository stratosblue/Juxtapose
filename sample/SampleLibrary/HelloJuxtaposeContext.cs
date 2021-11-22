using Juxtapose;
using Juxtapose.SourceGenerator;

namespace SampleLibrary
{
    [IllusionClass(typeof(IHello), typeof(Hello))]
    public partial class HelloJuxtaposeContext : JuxtaposeContext
    {
    }
}