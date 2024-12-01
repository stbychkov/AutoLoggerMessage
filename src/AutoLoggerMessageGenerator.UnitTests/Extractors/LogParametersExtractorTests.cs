using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.Import.Microsoft.Extensions.Telemetry.LoggerMessage;
using AutoLoggerMessageGenerator.Models;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

public class LogParametersExtractorTests : BaseSourceGeneratorTest
{
    [Fact]
    public void Extract_WithGivenMessageAndParameters_ShouldReturnParsedParameters()
    {
        var message = "The {EventName} was processed in {Time}ms";
        var parameters = "\"OrderReceived\", 40";

        var extensionDeclaration = "private static void Log<T1, T2>(string @message, T1 @arg1, T2 @arg2) {}";

        var (compilation, syntaxTree) = CompileSourceCode($"Log(\"{message}\", {parameters});", extensionDeclaration);
        var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

        var sut = new LogParametersExtractor(new LogPropertiesCheck(compilation));

        var result = sut.Extract(message, methodSymbol);

        result.Should().BeEquivalentTo(new LogCallParameter[]
        {
            new("global::System.String", "@EventName"),
            new("global::System.Int32", "@Time"),
        });
    }

    [Fact]
    public void Extract_WithGivenMessageAndNoParameters_ShouldReturnEmptyParameters()
    {
        var message = "Hello world!";

        var extensionDeclaration = "private static void Log(string @message) {}";

        var (compilation, syntaxTree) = CompileSourceCode($"Log(\"{message}\");", extensionDeclaration);
        var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

        var sut = new LogParametersExtractor(new LogPropertiesCheck(compilation));

        var result = sut.Extract(message, methodSymbol);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(ImmutableArray<LogCallParameter>.Empty);
    }
    
    [Fact]
    public void Extract_WithUtilityParameters_ShouldExtractAllParameters()
    {
        var message = "The {EventName} was processed in {Time}ms";
        var parameters = "\"OrderReceived\", 40";

        var extensionDeclaration = $$"""
                                     private static void Log<T1, T2>(
                                         EventId @{{LoggerExtensionsEmitter.EventIdArgument}}, 
                                         Exception @{{LoggerExtensionsEmitter.ExceptionArgumentName}}, 
                                         string @message, T1 @arg1, T2 @arg2) {}
                                     """;

        var (compilation, syntaxTree) = CompileSourceCode("""
                                                            Log(
                                                                new EventId(), 
                                                                new Exception(), 
                                                                "The {EventName} was processed in {Time}ms", 
                                                                "OrderReceived", 
                                                                40
                                                            );
                                                            """, extensionDeclaration);
        var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

        var sut = new LogParametersExtractor(new LogPropertiesCheck(compilation));

        var result = sut.Extract(message, methodSymbol);

        result.Should().BeEquivalentTo(new LogCallParameter[]
        {
            new("global::Microsoft.Extensions.Logging.EventId", $"@{LoggerExtensionsEmitter.EventIdArgument}"),
            new("global::System.Exception", $"@{LoggerExtensionsEmitter.ExceptionArgumentName}"),
            new("global::System.String", "@EventName"),
            new("global::System.Int32", "@Time"),
        });
    }
}
