using AutoLoggerMessageGenerator.Benchmarks;

ProjectConfiguration[] projectConfigurations =
[
    new()
    {
        Name = "MsExtensionLoggingConfiguration",
        References = new HashSet<string>
        {
            PackagesProvider.BenchmarkPackage,
            PackagesProvider.MicrosoftExtensionsLoggingPackage
        }
    },
    new()
    {
        Name = "MsExtensionsTelemetryConfiguration",
        References = new HashSet<string>
        {
            PackagesProvider.BenchmarkPackage,
            PackagesProvider.MicrosoftExtensionsLoggingPackage,
            PackagesProvider.MicrosoftExtensionsTelemetryPackage
        }
    },
    new()
    {
        Name = "AutoLoggerMessageGeneratorConfiguration",
        References = new HashSet<string>
        {
            PackagesProvider.BenchmarkPackage,
            PackagesProvider.MicrosoftExtensionsLoggingPackage,
            PackagesProvider.AutoLoggerMessageBuildOutput,
            PackagesProvider.AutoLoggerMessagePackage
        }
    },
    new()
    {
        Name = "AutoLoggerMessageGeneratorWithTelemetryConfiguration",
        References = new HashSet<string>
        {
            PackagesProvider.BenchmarkPackage,
            PackagesProvider.AutoLoggerMessagePackage,
            PackagesProvider.AutoLoggerMessageBuildOutput,
            PackagesProvider.MicrosoftExtensionsLoggingPackage,
            PackagesProvider.MicrosoftExtensionsTelemetryPackage,
        }
    }
];

var runner = new MultiBenchmarkRunner(projectConfigurations);
await runner.Run();
