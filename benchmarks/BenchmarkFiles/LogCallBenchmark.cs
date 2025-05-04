using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.Logging;

[RankColumn]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public partial class LogCallBenchmark
{
    private ILogger _logger = null!;
    private ConfigurationExample _configuration = null!;

    [GlobalSetup]
    public void Setup()
    {
        _logger = CustomNullLogger<LogCallBenchmark>.Instance;
        _configuration = new ConfigurationExample(
            0, "Root", new ConfigurationExample(
                1, "First Level", new ConfigurationExample(
                    2, "Second level", null)
            )
        );
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Without parameters")]
    public void LogInformationWithoutParameters()
    {
        _logger.LogInformation("Hello world!");
    }

#if DEFAULT && !TELEMETRY && !AUTO_LOGGER_MESSAGE

    [Benchmark]
    [BenchmarkCategory("Without parameters")]
    public void LoggerMessageWithoutParameters()
    {
        LoggerMessageWithoutParametersImpl();
    }

    [LoggerMessage(LogLevel.Information, Message = "Hello world!")]
    partial void LoggerMessageWithoutParametersImpl();

#endif

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("With 6 Parameters")]
    public void LogInformationWith6PrimitiveParameters()
    {
        _logger.LogInformation("Hello world! {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}", 1, 2, 3, 4, 5, 6);
    }

#if DEFAULT && !TELEMETRY && !AUTO_LOGGER_MESSAGE

    [Benchmark]
    [BenchmarkCategory("With 6 Parameters")]
    public void LoggerMessageWith6Parameters()
    {
        LoggerMessageWith6ParametersImpl(1, 2, 3, 4, 5, 6);
    }

    [LoggerMessage(LogLevel.Information, Message = "Hello world! {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}")]
    partial void LoggerMessageWith6ParametersImpl(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6);

#endif

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("With 7 Parameters")]
    public void LogInformationWith7PrimitiveParameters()
    {
        _logger.LogInformation("Hello world! {arg1} {arg2} {arg3} {arg4} {arg5} {arg6} {arg7}", 1, 2, 3, 4, 5, 6, 7);
    }

#if DEFAULT && !TELEMETRY && !AUTO_LOGGER_MESSAGE

    [Benchmark]
    [BenchmarkCategory("With 7 Parameters")]
    public void LoggerMessageWith7Parameters()
    {
        LoggerMessageWith7ParametersImpl(1, 2, 3, 4, 5, 6, 7);
    }

    [LoggerMessage(LogLevel.Information, Message = "Hello world! {arg1} {arg2} {arg3} {arg4} {arg5} {arg6} {arg7}")]
    partial void LoggerMessageWith7ParametersImpl(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6,
        int arg7);

#endif

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("With Complex Types Parameters")]
    public void LogInformationWithComplexParameters()
    {
        _logger.LogInformation("Hello world! {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}",
            _configuration, _configuration, _configuration, _configuration, _configuration, _configuration);
    }

#if DEFAULT && !TELEMETRY && !AUTO_LOGGER_MESSAGE
    [Benchmark]
    [BenchmarkCategory("With Complex Types Parameters")]
    public void LoggerMessageWithComplexParameters()
    {
        LoggerMessageWithComplexParametersImpl(
            _configuration, _configuration, _configuration,
            _configuration, _configuration, _configuration
        );
    }

    [LoggerMessage(LogLevel.Information, Message = "Hello world! {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}")]
    partial void LoggerMessageWithComplexParametersImpl(
        ConfigurationExample arg1,
        ConfigurationExample arg2,
        ConfigurationExample arg3,
        ConfigurationExample arg4,
        ConfigurationExample arg5,
        ConfigurationExample arg6
    );

#endif

#if TELEMETRY

    [Benchmark]
    [BenchmarkCategory("With Printable Complex Types Parameters")]
    public void LoggerMessageWithPrintableComplexParameters()
    {
        LoggerMessageWithPrintableComplexParametersImpl(
            _configuration, _configuration, _configuration,
            _configuration, _configuration, _configuration
        );
    }

    [LoggerMessage(LogLevel.Information, Message = "Hello world! {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}")]
    partial void LoggerMessageWithPrintableComplexParametersImpl(
        [LogProperties] ConfigurationExample arg1,
        [LogProperties] ConfigurationExample arg2,
        [LogProperties] ConfigurationExample arg3,
        [LogProperties] ConfigurationExample arg4,
        [LogProperties] ConfigurationExample arg5,
        [LogProperties] ConfigurationExample arg6
    );

#endif

    public record ConfigurationExample(int Id, string Name, ConfigurationExample? NestedConfiguration);
}
