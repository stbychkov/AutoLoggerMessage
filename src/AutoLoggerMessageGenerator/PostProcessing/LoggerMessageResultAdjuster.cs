using System.Text;

namespace AutoLoggerMessageGenerator.PostProcessing;

internal static class LoggerMessageResultAdjuster
{
    public static string? Adjust(string? generatedCode)
    {
        var builder = new StringBuilder(generatedCode).Replace(
            $"static partial void {Constants.LogMethodPrefix}",
            $"static void {Constants.LogMethodPrefix}"
        );

        #if EMBEDDED
        builder = builder.Replace(
            $"partial class {Constants.LoggerClassName}",
            $"{Constants.EmbeddedAttribute} partial class {Constants.LoggerClassName}"
        );
        #endif

        return builder.ToString();
    }
}
