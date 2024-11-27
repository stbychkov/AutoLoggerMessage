using System.Diagnostics;

namespace AutoLoggerMessageGenerator.Utilities;

internal static class DebugHelper
{
    [Conditional("DEBUG")]
    public static void Debug()
    {
        if (!Debugger.IsAttached)
            Debugger.Launch();
    }
}
