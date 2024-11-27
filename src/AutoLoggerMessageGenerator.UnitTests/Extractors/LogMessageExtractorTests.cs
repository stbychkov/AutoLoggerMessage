using AutoLoggerMessageGenerator.Extractors;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

public class LogMessageExtractorTests : BaseSourceGeneratorTest
{
    [Theory]
    [InlineData("Hello world {1}, {2}!")]
    [InlineData("")]
    public void Extract_WithPassingLiteralValues_ShouldReturnExpectedMessage(string expectedMessage)
    {
        var (compilation, syntaxTree) = CompileSourceCode($"{LoggerName}.LogInformation(\"{expectedMessage}\");");

        var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

        var result = LogMessageExtractor.Extract(methodSymbol, invocationExpression, semanticModel);

        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void Extract_WithPassingConstClassMembers_ShouldReturnExpectedMessage()
    {
        var expectedMessage = "Hello world";
        var (compilation, syntaxTree) = CompileSourceCode(
            $"{LoggerName}.LogInformation(Message);",
            $"private const string Message = \"{expectedMessage}\";"
        );

        var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

        var result = LogMessageExtractor.Extract(methodSymbol, invocationExpression, semanticModel);

        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void Extract_WithPassingLocalConstVariable_ShouldReturnExpectedMessage()
    {
        var expectedMessage = "Hello world";
        var (compilation, syntaxTree) = CompileSourceCode($"""
                                                          const string message = "{expectedMessage}";
                                                          {LoggerName}.LogInformation(message);
                                                          """
        );

        var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

        var result = LogMessageExtractor.Extract(methodSymbol, invocationExpression, semanticModel);

        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void Extract_WithPassingNullValues_ShouldReturnNull()
    {
        var (compilation, syntaxTree) = CompileSourceCode($"{LoggerName}.LogInformation(null);");

        var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

        var result = LogMessageExtractor.Extract(methodSymbol, invocationExpression, semanticModel);

        result.Should().Be(null);
    }
}
