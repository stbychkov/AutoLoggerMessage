using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.Models;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

public class LogParametersExtractorTests : BaseSourceGeneratorTest
{
    [Fact]
    public void Extract_WithGivenMessageAndParameters_ShouldReturnMessageAndParsedParameters()
    {
        const string message = "The {EventName} was processed in {Time}ms";
        const string parameters = $"""
                                   "{message}", "OrderReceived", 40
                                   """;

        const string extensionDeclaration = $$"""
                                              private static void Log<T1, T2>(
                                                  string {{MessageArgumentName}},
                                                  T1 {{ArgumentName}}1,
                                                  T2 {{ArgumentName}}2) {}
                                              """;

        var (compilation, syntaxTree) = CompileSourceCode($"Log({parameters});", extensionDeclaration);
        var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = (IMethodSymbol) semanticModel.GetSymbolInfo(invocationExpression).Symbol!;

        var sut = new LogParametersExtractor();

        var result = sut.Extract(message, methodSymbol);

        result.Should().BeEquivalentTo(new LogCallParameter[]
        {
            new("global::System.String", MessageArgumentName),
            new("global::System.String", "@EventName"),
            new("global::System.Int32", "@Time"),
        });
    }

    [Fact]
    public void Extract_WithGivenMessageAndNoParameters_ShouldReturnOnlyMessageParameter()
    {
        const string message = "Hello world!";

        const string extensionDeclaration = $"private static void Log(string {MessageArgumentName}) {{}}";

        var (compilation, syntaxTree) = CompileSourceCode($"""Log("{message}");""", extensionDeclaration);
        var (_, methodSymbol, _) = FindLoggerMethodInvocation(compilation, syntaxTree);

        var sut = new LogParametersExtractor();

        var result = sut.Extract(message, methodSymbol!);

        result.Should().NotBeNull();
        result.HasValue.Should().BeTrue();
        result!.Value.Length.Should().Be(1);
        result.Value[0].Should().BeEquivalentTo(
            new LogCallParameter(
                "global::System.String",
                MessageArgumentName
            )
        );
    }

    [Fact]
    public void Extract_WithUtilityParameters_ShouldExtractAllParameters()
    {
        const string message = "The {EventName} was processed in {Time}ms";

        const string extensionDeclaration = $$"""
                                              private static void Log<T1, T2>(
                                                  LogLevel {{LogLevelArgumentName}},
                                                  EventId {{EventIdArgumentName}},
                                                  Exception {{ExceptionArgumentName}},
                                                  string {{MessageArgumentName}},
                                                  T1 {{ArgumentName}}1,
                                                  T2 {{ArgumentName}}2) {}
                                              """;

        var (compilation, syntaxTree) = CompileSourceCode("""
                                                            Log(
                                                                LogLevel.Information,
                                                                new EventId(),
                                                                new Exception(),
                                                                "The {EventName} was processed in {Time}ms",
                                                                "OrderReceived",
                                                                40
                                                            );
                                                            """, extensionDeclaration);
        var (_, methodSymbol, _) = FindLoggerMethodInvocation(compilation, syntaxTree);

        var sut = new LogParametersExtractor();

        var result = sut.Extract(message, methodSymbol!);

        result.Should().BeEquivalentTo(new LogCallParameter[]
        {
            new("global::Microsoft.Extensions.Logging.LogLevel", LogLevelArgumentName),
            new("global::Microsoft.Extensions.Logging.EventId", EventIdArgumentName),
            new("global::System.Exception", ExceptionArgumentName),
            new("global::System.String", MessageArgumentName),
            new("global::System.String", "@EventName"),
            new("global::System.Int32", "@Time"),
        });
    }
}
