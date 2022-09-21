using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Juxtapose.SourceGenerator;

internal static class DebuggerLauncher
{
    #region Public 方法

    [Conditional("DEBUG")]
    public static void TryLaunch(GeneratorExecutionContext context) => TryLaunch(context.AnalyzerConfigOptions);

    [Conditional("DEBUG")]
    public static void TryLaunch(AnalyzerConfigOptionsProvider configOptionsProvider)
    {
#if DEBUG
        if (!Debugger.IsAttached
            && configOptionsProvider.IsMSBuildSwitchOn("LunchDebuggerInSourceGenerator"))
        {
            Debugger.Launch();
        }
#endif
    }

    #endregion Public 方法
}