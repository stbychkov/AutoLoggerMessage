using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.VirtualLoggerMessage;
using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.UnitTests.VirtualLoggerMessage;

internal class VirtualLoggerMessageClassBuilderTests
{
    private readonly LogMessageCall _logMessageCall = new()
    {
        Id = Guid.NewGuid(),
        Location = new CallLocation
        {
            FilePath = "./some/path/to/file.cs",
            Line = 2,
            Character = 22
        },
        Message = "Hello, World!",
        MethodName = "LogCall",
        Namespace = "SomeNamespace",
        ClassName = "SomeClass",
        LogLevel = "Critical",
        Parameters =
        [
            new CallParameter("int", "@intParam", CallParameterType.Others),
            new CallParameter("string", "@stringParam", CallParameterType.Others),
            new CallParameter("bool", "@boolParam", CallParameterType.Others),
            new CallParameter("SomeClass", "@classParam", CallParameterType.Others, true),
            new CallParameter("SomeStruct", "@structParam", CallParameterType.Others, true)
        ]
    };

    [Test]
    [MethodDataSource(nameof(TestConfigurations))]
    public async Task Build_WithDifferentConfiguration_ShouldReturnLegitLoggerMessageDeclaration(
        string description, SourceGeneratorConfiguration configuration, bool useTelemetryExtensions)
    {
        var sut = new VirtualLoggerMessageClassBuilder(configuration, useTelemetryExtensions);

        var result = sut.Build([
            _logMessageCall,
            _logMessageCall with
            {
                Id = Guid.NewGuid(),
                Location = MockLogCallLocationBuilder.Build("path/to/another/file.cs", 3, 33),
                Message = "Goodbye, World!",
                LogLevel = "Trace"
            }
        ]);

        var syntaxTree = (await result.SyntaxTree.GetRootAsync()).NormalizeWhitespace().ToFullString();

        await Verify(syntaxTree)
            .UseTextForParameters(description)
            .ScrubInlineGuids();
    }

    [Test]
    public async Task Build_WithEscapeSequences_ShouldBuildAsItIs()
    {
        var sut = new VirtualLoggerMessageClassBuilder(default);

        var result = sut.Build([
            _logMessageCall with
            {
                Id = Guid.NewGuid(),
                Location = MockLogCallLocationBuilder.Build("path/to/another/file.cs", 3, 33),
                Message = "All characters should be passed as a string literal expression: \n\r\t",
                LogLevel = "Trace"
            }
        ]);

        var syntaxTree = (await result.SyntaxTree.GetRootAsync()).NormalizeWhitespace().ToFullString();

        await Verify(syntaxTree).ScrubInlineGuids();
    }

    public static IEnumerable<Func<(string, SourceGeneratorConfiguration, bool)>> TestConfigurations() =>
    [
        () => (
            "all_enabled",
            new SourceGeneratorConfiguration(
                GenerateInterceptorAttribute: false,
                GenerateSkipEnabledCheck: true,
                GenerateOmitReferenceName: true,
                GenerateSkipNullProperties: true,
                GenerateTransitive: true,
                OverrideBeginScopeBehavior: false
            ), true),
        () => (
            "telemetry_disabled",
            new SourceGeneratorConfiguration(
                GenerateInterceptorAttribute: false,
                GenerateSkipEnabledCheck: true,
                GenerateOmitReferenceName: true,
                GenerateSkipNullProperties: true,
                GenerateTransitive: true,
                OverrideBeginScopeBehavior: false),
            false),
        () => (
            "only_telemetry_enabled",
            new SourceGeneratorConfiguration(
                GenerateInterceptorAttribute: false,
                GenerateSkipEnabledCheck: false,
                GenerateOmitReferenceName: false,
                GenerateSkipNullProperties: false,
                GenerateTransitive: false,
                OverrideBeginScopeBehavior: false),
            true),
        () => (
            "all_disabled",
            new SourceGeneratorConfiguration(
                GenerateInterceptorAttribute: false,
                GenerateSkipEnabledCheck: false,
                GenerateOmitReferenceName: false,
                GenerateSkipNullProperties: false,
                GenerateTransitive: false,
                OverrideBeginScopeBehavior: false),
            false),
    ];
}
