using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.Logging;

[RankColumn]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public partial class LogScopeBenchmark
{
    private ILogger _logger = null!;
    private ConfigurationExample _configuration = null!;

    [GlobalSetup]
    public void Setup()
    {
        _logger = CustomNullLogger<LogScopeBenchmark>.Instance;
        _configuration = new ConfigurationExample(
            0, "Root", new ConfigurationExample(
                1, "First Level", new ConfigurationExample(
                    2, "Second level", null)
            )
        );
    }

#if !TELEMETRY

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Without parameters")]
    public void BeginScopeWithoutParameters()
    {
        using var _ = _logger.BeginScope("Hello world!");
    }

#endif

#if DEFAULT && !TELEMETRY && !AUTO_LOGGER_MESSAGE

    [Benchmark]
    [BenchmarkCategory("Without parameters")]
    public void LoggerDefineScopeWithoutParameters()
    {
        using var _ = _loggerDefineScopeWithoutParameters(_logger);
    }

    private static readonly Func<ILogger, IDisposable?> _loggerDefineScopeWithoutParameters =
        LoggerMessage.DefineScope("Hello world!");

#endif

#if !TELEMETRY
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("With 6 Parameters")]
    public void BeginScopeWith6PrimitiveParameters()
    {
        using var _ = _logger.BeginScope("Hello world! {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}", 1, 2, 3, 4, 5, 6);
    }
#endif

#if DEFAULT && !TELEMETRY && !AUTO_LOGGER_MESSAGE

    [Benchmark]
    [BenchmarkCategory("With 6 Parameters")]
    public void LoggerDefineScopeWith6Parameters()
    {
        using var _ = _loggerDefineScopeWith6Parameters(_logger, 1, 2, 3, 4, 5, 6);
    }

    private static readonly Func<ILogger, int, int, int, int, int, int, IDisposable?> _loggerDefineScopeWith6Parameters =
        LoggerMessage.DefineScope<int, int, int, int, int, int>("Hello world! {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}");

#endif

#if !TELEMETRY

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("With Complex Types Parameters")]
    public void BeginScopeWithComplexParameters()
    {
        using var _ = _logger.BeginScope("Hello world! {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}",
            _configuration, _configuration, _configuration, _configuration, _configuration, _configuration);
    }

#endif

#if DEFAULT && !TELEMETRY && !AUTO_LOGGER_MESSAGE

    [Benchmark]
    [BenchmarkCategory("With Complex Types Parameters")]
    public void LoggerDefineScopeWithComplexParameters()
    {
        using var _ = _loggerDefineScopeWithComplexParameters(_logger,
            _configuration, _configuration, _configuration,
            _configuration, _configuration, _configuration
        );
    }

    private static readonly Func<ILogger, ConfigurationExample, ConfigurationExample,
        ConfigurationExample, ConfigurationExample, ConfigurationExample,
        ConfigurationExample, IDisposable?> _loggerDefineScopeWithComplexParameters =
        LoggerMessage.DefineScope<ConfigurationExample, ConfigurationExample,
            ConfigurationExample, ConfigurationExample,
            ConfigurationExample, ConfigurationExample>(
            "Hello world! {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}"
        );

#endif

    public record ConfigurationExample(int Id, string Name, ConfigurationExample? NestedConfiguration);
}
