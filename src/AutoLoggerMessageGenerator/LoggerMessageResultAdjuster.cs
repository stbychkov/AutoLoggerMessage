namespace AutoLoggerMessageGenerator;

internal static class LoggerMessageResultAdjuster
{
    public static string? Adjust(string? generatedCode) =>
        generatedCode?.Replace($"static partial void {Constants.LogMethodPrefix}", $"static void {Constants.LogMethodPrefix}");
}
