using AutoLoggerMessageGenerator.Extractors;
using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

internal class LogLevelExtractorTests : BaseSourceGeneratorTest
{
    [Test]
    [Arguments("LogTrace(default)", nameof(LogLevel.Trace))]
    [Arguments("LogDebug(default)", nameof(LogLevel.Debug))]
    [Arguments("LogInformation(default)", nameof(LogLevel.Information))]
    [Arguments("LogWarning(default)", nameof(LogLevel.Warning))]
    [Arguments("LogError(default)", nameof(LogLevel.Error))]
    [Arguments("LogCritical(default)", nameof(LogLevel.Critical))]

    [Arguments("Log(LogLevel.Trace, default)", nameof(LogLevel.Trace))]
    [Arguments("Log(LogLevel.Debug, default)", nameof(LogLevel.Debug))]
    [Arguments("Log(LogLevel.Information, default)", nameof(LogLevel.Information))]
    [Arguments("Log(LogLevel.Warning, default)", nameof(LogLevel.Warning))]
    [Arguments("Log(LogLevel.Error, default)", nameof(LogLevel.Error))]
    [Arguments("Log(LogLevel.Critical, default)", nameof(LogLevel.Critical))]

    [Arguments("AnyOtherMethod(default)", null)]
    public async Task Extract_WithGivenMethodCall_ShouldReturnExpectedLogLevel(string methodCall, string? expectedLogLevel)
    {
        var (compilation, syntaxTree) = await CompileSourceCode($"{LoggerName}.{methodCall};");
        var (invocationExpression, methodSymbol, _) = FindMethodInvocation(compilation, syntaxTree);

        var result = LogLevelExtractor.Extract(methodSymbol!, invocationExpression);

        await Assert.That(result).IsEqualTo(expectedLogLevel);
    }

    [Test]
    public async Task Extract_WithNotConstantLogLevel_ShouldReturnNull()
    {
        var sourceCode = """
                         var logLevel = DateTime.Now.Ticks % 2 == 0 ? LogLevel.Warning : LogLevel.Error;
                         Log(logLevel, default);
                         """;
        var (compilation, syntaxTree) = await CompileSourceCode(sourceCode);
        var (invocationExpression, methodSymbol, _) = FindMethodInvocation(compilation, syntaxTree);

        var result = LogLevelExtractor.Extract(methodSymbol!, invocationExpression);

        await Assert.That(result).IsEqualTo(null);
    }
}
