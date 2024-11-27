using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.VirtualLoggerMessage;
using Microsoft.CodeAnalysis;

#pragma warning disable xUnit1039

namespace AutoLoggerMessageGenerator.UnitTests.VirtualLoggerMessage;

public class VirtualLoggerMessageClassBuilderTests
{
    private LogCall _logCall = new()
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
            new LogCallParameter("int", "intParam", false),
            new LogCallParameter("string", "stringParam", false),
            new LogCallParameter("bool", "boolParam", false),
            new LogCallParameter("SomeClass", "classParam", true),
            new LogCallParameter("SomeStruct", "structParam", true)
        ]
    }; 
    
    [Theory]
    [MemberData(nameof(TestConfigurations))]
    internal async Task Build_WithDifferentConfiguration_ShouldReturnLegitLoggerMessageDeclaration(
        SourceGeneratorConfiguration configuration, bool useTelemetryExtensions)
    {
        var sut = new VirtualLoggerMessageClassBuilder(configuration, useTelemetryExtensions);
        
        var result = sut.Build([
            _logCall, 
            _logCall with
            {
                Id = Guid.NewGuid(),
                Location = new LogCallLocation("path/to/another/file.cs", 3, 33, default),
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

    public static TheoryData<object, bool> TestConfigurations =
        new()
        {
            { new SourceGeneratorConfiguration(default, true, true, true, true), true },
            { new SourceGeneratorConfiguration(default, true, true, true, true), false },
            { new SourceGeneratorConfiguration(default, false, false, false, false), true },
            { new SourceGeneratorConfiguration(default, false, false, false, false), false },
        };
}
