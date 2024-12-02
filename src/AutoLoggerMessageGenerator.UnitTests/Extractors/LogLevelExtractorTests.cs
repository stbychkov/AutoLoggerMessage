using AutoLoggerMessageGenerator.Extractors;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

public class LogLevelExtractorTests : BaseSourceGeneratorTest
{
    [Theory]
    [InlineData("LogTrace(default)", nameof(LogLevel.Trace))]
    [InlineData("LogDebug(default)", nameof(LogLevel.Debug))]
    [InlineData("LogInformation(default)", nameof(LogLevel.Information))]
    [InlineData("LogWarning(default)", nameof(LogLevel.Warning))]
    [InlineData("LogError(default)", nameof(LogLevel.Error))]
    [InlineData("LogCritical(default)", nameof(LogLevel.Critical))]

    [InlineData("Log(LogLevel.Trace, default)", nameof(LogLevel.Trace))]
    [InlineData("Log(LogLevel.Debug, default)", nameof(LogLevel.Debug))]
    [InlineData("Log(LogLevel.Information, default)", nameof(LogLevel.Information))]
    [InlineData("Log(LogLevel.Warning, default)", nameof(LogLevel.Warning))]
    [InlineData("Log(LogLevel.Error, default)", nameof(LogLevel.Error))]
    [InlineData("Log(LogLevel.Critical, default)", nameof(LogLevel.Critical))]

    [InlineData("AnyOtherMethod(default)", null)]
    public void Extract_WithGivenMethodCall_ShouldReturnExpectedLogLevel(string methodCall, string? expectedLogLevel)
    {
        var (compilation, syntaxTree) = CompileSourceCode($"{LoggerName}.{methodCall};");
        var (invocationExpression, methodSymbol, _) = FindLoggerMethodInvocation(compilation, syntaxTree);

        var result = LogLevelExtractor.Extract(methodSymbol!, invocationExpression);

        result.Should().Be(expectedLogLevel);
    }
}
