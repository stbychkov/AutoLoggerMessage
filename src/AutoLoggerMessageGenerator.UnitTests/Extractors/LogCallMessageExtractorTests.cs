using AutoLoggerMessageGenerator.Extractors;
using FluentAssertions;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

public class LogCallMessageExtractorTests : BaseSourceGeneratorTest
{
    [Theory]
    [InlineData("Hello world {1}, {2}!")]
    [InlineData("")]
    public void Extract_WithPassingLiteralValues_ShouldReturnExpectedMessage(string expectedMessage)
    {
        var (compilation, syntaxTree) = CompileSourceCode($"""{LoggerName}.LogInformation("{expectedMessage}");""");
        var (invocationExpression, methodSymbol, semanticModel) = FindLoggerMethodInvocation(compilation, syntaxTree);

        var result = LogCallMessageExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void Extract_WithPassingConstClassMembers_ShouldReturnExpectedMessage()
    {
        var expectedMessage = "Hello world";
        var (compilation, syntaxTree) = CompileSourceCode(
            $"{LoggerName}.LogInformation(Message);",
            $"""private const string Message = "{expectedMessage}";"""
        );
        var (invocationExpression, methodSymbol, semanticModel) = FindLoggerMethodInvocation(compilation, syntaxTree);

        var result = LogCallMessageExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

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
        var (invocationExpression, methodSymbol, semanticModel) = FindLoggerMethodInvocation(compilation, syntaxTree);

        var result = LogCallMessageExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void Extract_WithPassingNullValues_ShouldReturnNull()
    {
        var (compilation, syntaxTree) = CompileSourceCode($"{LoggerName}.LogInformation(null);");
        var (invocationExpression, methodSymbol, semanticModel) = FindLoggerMethodInvocation(compilation, syntaxTree);

        var result = LogCallMessageExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

        result.Should().Be(null);
    }

    [Fact]
    public void Extract_WithBinaryExpressions_ShouldReturnConcatenatedString()
    {
        const string part1 = "Hello ";

        var sourceCode = $"""
                          const string part2 = "world! ";
                          const int parameter = 42;
                          {LoggerName}.LogInformation("{part1}" + part2 + parameter);
                          """;

        var (compilation, syntaxTree) = CompileSourceCode(sourceCode);
        var (invocationExpression, methodSymbol, semanticModel) = FindLoggerMethodInvocation(compilation, syntaxTree);

        var result = LogCallMessageExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

        result.Should().Be("Hello world! 42");
    }

    [Fact]
    public void Extract_WithBinaryNonConstantExpressions_ShouldReturnNull()
    {
        const string part1 = "Hello ";

        var sourceCode = $"""
                          string part2 = "world! ";
                          int parameter = 42;
                          {LoggerName}.LogInformation("{part1}" + part2 + parameter);
                          """;

        var (compilation, syntaxTree) = CompileSourceCode(sourceCode);
        var (invocationExpression, methodSymbol, semanticModel) = FindLoggerMethodInvocation(compilation, syntaxTree);

        var result = LogCallMessageExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

        result.Should().Be(null);
    }
}
