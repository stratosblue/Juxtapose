using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Juxtapose.SourceGenerator;

internal static class DebuggerLauncher
{
    #region Public 方法

    [Conditional("DEBUG")]
    public static void TryLaunch(GeneratorExecutionContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached
            && context.IsMSBuildSwitchOn("LunchDebuggerInSourceGenerator"))
        {
            Debugger.Launch();
        }
#endif
    }

    #endregion Public 方法
}