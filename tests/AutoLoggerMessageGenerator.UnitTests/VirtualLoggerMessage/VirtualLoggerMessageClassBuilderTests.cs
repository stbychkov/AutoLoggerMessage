using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.VirtualLoggerMessage;
using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.UnitTests.VirtualLoggerMessage;

internal class VirtualLoggerMessageClassBuilderTests
{
    private readonly LogCall _logCall = new()
    {
        Id = Guid.NewGuid(),
        Location = new LogCallLocation
        {
            FilePath = "./some/path/to/file.cs",
            Line = 2,
            Character = 22
        },
        Message = "Hello, World!",
        Name = "LogCall",
        Namespace = "SomeNamespace",
        ClassName = "SomeClass",
        LogLevel = "Critical",
        Parameters = [
            new LogCallParameter("int", "@intParam", LogCallParameterType.Others),
            new LogCallParameter("string", "@stringParam", LogCallParameterType.Others),
            new LogCallParameter("bool", "@boolParam", LogCallParameterType.Others),
            new LogCallParameter("SomeClass", "@classParam", LogCallParameterType.Others, true),
            new LogCallParameter("SomeStruct", "@structParam", LogCallParameterType.Others, true)
        ]
    };

    [Test]
    [MethodDataSource(nameof(TestConfigurations))]
    public async Task Build_WithDifferentConfiguration_ShouldReturnLegitLoggerMessageDeclaration(
        SourceGeneratorConfiguration configuration, bool useTelemetryExtensions)
    {
        var sut = new VirtualLoggerMessageClassBuilder(configuration, useTelemetryExtensions);

        var result = sut.Build([
            _logCall,
            _logCall with
            {
                Id = Guid.NewGuid(),
                Location = MockLogCallLocationBuilder.Build("path/to/another/file.cs", 3, 33),
                Message = "Goodbye, World!",
                LogLevel = "Trace"
            }
        ]);

        var syntaxTree = (await result.SyntaxTree.GetRootAsync()).NormalizeWhitespace().ToFullString();

        await Verify(syntaxTree)
            .UseParameters(configuration, useTelemetryExtensions)
            .HashParameters()
            .ScrubInlineGuids();
    }

    [Test]
    public async Task Build_WithEscapeSequences_ShouldBuildAsItIs()
    {
        var sut = new VirtualLoggerMessageClassBuilder(default);

        var result = sut.Build([
            _logCall with
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

    public static IEnumerable<Func<(SourceGeneratorConfiguration, bool)>> TestConfigurations() =>
    [
        () => (new SourceGeneratorConfiguration(default, true, true, true, true), true),
        () => (new SourceGeneratorConfiguration(default, true, true, true, true), false),
        () => (new SourceGeneratorConfiguration(default, false, false, false, false), true),
        () => (new SourceGeneratorConfiguration(default, false, false, false, false), false),
    ];
}
