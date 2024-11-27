namespace AutoLoggerMessageGenerator.Configuration;

internal static class SourceGeneratorConfigurationExtensions
{
    public static string ToLowerBooleanString(this bool value) => value ? "true" : "false";
}
