using System.Diagnostics;
using System.Threading.Tasks;

namespace Juxtapose.Test.RunningHost;

internal class Program
{
    #region Private 方法

    private static async Task Main(string[] args)
    {
        if (!Debugger.IsAttached)
        {
            //Debugger.Launch();
        }

        //GreeterJuxtaposeContext.SharedInstance.FastSetFileLoggerFactory(LogLevel.Trace);
        GreeterJuxtaposeContext.SharedInstance.UnSetConsoleLoggerFactory();

        await JuxtaposeEntryPoint.AsEndpointAsync(args);
    }

    #endregion Private 方法
}