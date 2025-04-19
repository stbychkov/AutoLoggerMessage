namespace AutoLoggerMessageGenerator.Benchmarks;

internal static class PackagesProvider
{
    public const string BenchmarkPackage = """<PackageReference Include="BenchmarkDotNet"/>""";
    public const string MicrosoftExtensionsLoggingPackage = """<PackageReference Include="Microsoft.Extensions.Logging.Abstractions"/>""";
    public const string MicrosoftExtensionsTelemetryPackage = """<PackageReference Include="Microsoft.Extensions.Telemetry.Abstractions"/>""";
    public const string AutoLoggerMessagePackage = """<ProjectReference Include="..\..\..\..\..\..\AutoLoggerMessageGenerator\AutoLoggerMessageGenerator.csproj" ReferenceOutputAssembly="true" OutputItemType="Analyzer" />""";
}
