using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Caching;
using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.ReferenceAnalyzer;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoLoggerMessageGenerator.UnitTests.Caching;

using InputSource = (Compilation Compilation, (SourceGeneratorConfiguration Configuration, (ImmutableArray<Reference> References, ImmutableArray<LogCall> LogCalls) Others) Others);

public class InputSourceComparerTests
{
    [Fact]
    public void Equals_WithDifferentCompilation_ShouldReturnTrue()
    {
        var compilation1 = CSharpCompilation.Create(default);
        var compilation2 = CSharpCompilation.Create(default);
        
        var inputSource1  = CreateInputSource(compilation: compilation1);
        var inputSource2 = CreateInputSource(compilation: compilation2);

        var sut = new InputSourceComparer();

        sut.Equals(inputSource1, inputSource2).Should().BeTrue();
    }
    
    [Fact]
    public void Equals_WithDifferentConfiguration_ShouldReturnFalse()
    {
        var configuration1 = new SourceGeneratorConfiguration(true, true, true, true, true);
        var configuration2 = new SourceGeneratorConfiguration(false, false, false, false, false);
        
        var inputSource1  = CreateInputSource(configuration: configuration1);
        var inputSource2 = CreateInputSource(configuration: configuration2);

        var sut = new InputSourceComparer();

        sut.Equals(inputSource1, inputSource2).Should().BeFalse();
    }
    
    [Theory]
    [InlineData("ref1", "1.0.0", "ref2", "1.0.0")]
    [InlineData("ref1", "1.0.0", "ref1", "2.0.0")]
    public void Equals_WithDifferentReferences_ShouldReturnFalse(
        string reference1Name, string reference1Version, 
        string reference2Name, string reference2Version)
    {
        ImmutableArray<Reference> references1 = [new Reference(reference1Name, new Version(reference1Version))];
        ImmutableArray<Reference> references2 = [new Reference(reference2Name, new Version(reference2Version))];
        
        var inputSource1  = CreateInputSource(references: references1);
        var inputSource2 = CreateInputSource(references: references2);

        var sut = new InputSourceComparer();

        sut.Equals(inputSource1, inputSource2).Should().BeFalse();
    }
    
    [Fact]
    public void Equals_WithDifferentLogCalls_ShouldReturnFalse()
    {
        ImmutableArray<LogCall> logCalls = [new LogCall { Message = "message1"}];
        ImmutableArray<LogCall> logCalls2 = [new LogCall { Message = "message2"}];
        
        var inputSource1  = CreateInputSource(logCalls: logCalls);
        var inputSource2 = CreateInputSource(logCalls: logCalls2);

        var sut = new InputSourceComparer();

        sut.Equals(inputSource1, inputSource2).Should().BeFalse();
    }

    private static InputSource CreateInputSource(Compilation? compilation = default, 
        SourceGeneratorConfiguration? configuration = default,
        ImmutableArray<Reference>? references = default, 
        ImmutableArray<LogCall>? logCalls = default)
    {
        compilation ??= CSharpCompilation.Create(default);
        configuration ??= new SourceGeneratorConfiguration(true, true, true, true, true);
        references ??= [new Reference("some lib", new Version("1.2.3"))];
        logCalls ??=
        [
            new LogCall(Guid.NewGuid(), new LogCallLocation("some file", 1, 2, Location.None), "namespace", "class", "name",
                "information", "message", [])
        ];

        return new InputSource(compilation, (configuration.Value, (references.Value, logCalls.Value)));
    }
}
