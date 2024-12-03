using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.Models;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

public class LogCallParametersExtractorTests : BaseSourceGeneratorTest
{
    [Fact]
    public void Extract_WithGivenMessageAndParameters_ShouldReturnMessageAndParsedParameters()
    {
        const string message = "The {EventName} was processed in {Time}ms";
        const string parameters = $"""
                                   "{message}", "OrderReceived", 40
                                   """;

        string extensionDeclaration = $$"""
                                        private static void Log<T1, T2>(
                                            string {{MessageParameterName}},
                                            T1 {{ParameterName}}1,
                                            T2 {{ParameterName}}2) {}
                                        """;

        var (compilation, syntaxTree) = CompileSourceCode($"Log({parameters});", extensionDeclaration);
        var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invocationExpression).Symbol!;

        var sut = new LogCallParametersExtractor();

        var result = sut.Extract(message, methodSymbol);

        result.Should().BeEquivalentTo(new LogCallParameter[]
        {
            new("global::System.String", MessageParameterName, LogCallParameterType.Message),
            new("global::System.String", "@EventName", LogCallParameterType.Others),
            new("global::System.Int32", "@Time", LogCallParameterType.Others),
        });
    }

    [Fact]
    public void Extract_WithGivenMessageAndNoParameters_ShouldReturnOnlyMessageParameter()
    {
        const string message = "Hello world!";

        string extensionDeclaration = $"private static void Log(string {MessageParameterName}) {{}}";

        var (compilation, syntaxTree) = CompileSourceCode($"""Log("{message}");""", extensionDeclaration);
        var (_, methodSymbol, _) = FindLoggerMethodInvocation(compilation, syntaxTree);

        var sut = new LogCallParametersExtractor();

        var result = sut.Extract(message, methodSymbol!);

        result.Should().NotBeNull();
        result.HasValue.Should().BeTrue();
        result!.Value.Length.Should().Be(1);
        result.Value[0].Should().BeEquivalentTo(
            new LogCallParameter(
                "global::System.String",
                MessageParameterName,
                LogCallParameterType.Message
            )
        );
    }

    [Fact]
    public void Extract_WithUtilityParameters_ShouldExtractAllParameters()
    {
        const string message = "The {EventName} was processed in {Time}ms";

        string extensionDeclaration = $$"""
                                        private static void Log<T1, T2>(
                                            LogLevel {{LogLevelParameterName}},
                                            EventId {{EventIdParameterName}},
                                            Exception {{ExceptionParameterName}},
                                            string {{MessageParameterName}},
                                            T1 {{ParameterName}}1,
                                            T2 {{ParameterName}}2) {}
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

        var sut = new LogCallParametersExtractor();

        var result = sut.Extract(message, methodSymbol!);

        result.Should().BeEquivalentTo(new LogCallParameter[]
        {
            new("global::Microsoft.Extensions.Logging.LogLevel", LogLevelParameterName, LogCallParameterType.LogLevel),
            new("global::Microsoft.Extensions.Logging.EventId", EventIdParameterName, LogCallParameterType.EventId),
            new("global::System.Exception", ExceptionParameterName, LogCallParameterType.Exception),
            new("global::System.String", MessageParameterName, LogCallParameterType.Message),
            new("global::System.String", "@EventName", LogCallParameterType.Others),
            new("global::System.Int32", "@Time", LogCallParameterType.Others),
        });
    }

    [Fact]
    public void Extract_WithUsingReservedParameterNamesInTemplate_ShouldTransformReservedTemplateParameterNamesToMakeThemUnique()
    {
        const string message = "{@eventId_} The {message} was processed in {time}ms";
        const string parameters = $"""
                                   new EventId(), "{message}", 1, "OrderReceived", 40
                                   """;

        const string extensionDeclaration = $$"""
                                              private static void Log<T1, T2, T3>(
                                                  EventId {{EventIdParameterName}},
                                                  string {{MessageParameterName}},
                                                  T1 {{ParameterName}}1,
                                                  T2 {{ParameterName}}2,
                                                  T3 {{ParameterName}}3) {}
                                              """;

        var (compilation, syntaxTree) = CompileSourceCode($"Log({parameters});", extensionDeclaration);
        var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invocationExpression).Symbol!;

        var sut = new LogCallParametersExtractor();

        var result = sut.Extract(message, methodSymbol);

        result.Should().BeEquivalentTo(new LogCallParameter[]
        {
            new("global::Microsoft.Extensions.Logging.EventId", "@eventId__", LogCallParameterType.EventId),
            new("global::System.String", "@message__", LogCallParameterType.Message),
            new("global::System.Int32", "@eventId_", LogCallParameterType.Others),
            new("global::System.String", "@message", LogCallParameterType.Others),
            new("global::System.Int32", "@time", LogCallParameterType.Others),
        });
    }
}
