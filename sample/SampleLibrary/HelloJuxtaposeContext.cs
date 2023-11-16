using Juxtapose;
using Juxtapose.SourceGenerator;

namespace SampleLibrary
{
    [Illusion(typeof(Hello))]
    public partial class HelloJuxtaposeContext : JuxtaposeContext
    {
    }
}

namespace SampleLibrary
{
    public sealed partial class HelloIllusion : IHello
    { }
}
