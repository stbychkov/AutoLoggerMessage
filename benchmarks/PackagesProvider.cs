namespace AutoLoggerMessageGenerator.Benchmarks;

internal static class PackagesProvider
{
    public const string BenchmarkPackage = """<PackageReference Include="BenchmarkDotNet"/>""";
    public const string MicrosoftExtensionsLoggingPackage = """<PackageReference Include="Microsoft.Extensions.Logging.Abstractions"/>""";
    public const string MicrosoftExtensionsTelemetryPackage = """<PackageReference Include="Microsoft.Extensions.Telemetry.Abstractions"/>""";
    public const string AutoLoggerMessageBuildOutput = """<Reference Include="AutoLoggerMessageGenerator.BuildOutput" HintPath="..\AutoLoggerMessageGenerator.BuildOutput.dll"/>""";
    public const string AutoLoggerMessagePackage = """<Analyzer Include="..\AutoLoggerMessageGenerator.dll"/>""";
}
