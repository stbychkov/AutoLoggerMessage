using AutoLoggerMessageGenerator.Extractors;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

public class LogCallCallerExtractorTests : BaseSourceGeneratorTest
{
    [Theory]
    [InlineData(false, Namespace, ClassName)]
    [InlineData(true, "", ClassName)]
    public void Extract_WithLogCallAndGivenNamespace_ShouldReturnExpectedNamespaceAndClassName(
        bool useGlobalNamespace, string expectedNamespace, string expectedClassName)
    {
        var message = $$"""{{LoggerName}}.LogInformation("Hello world");""";
        var (_, syntaxTree) = CompileSourceCode(message, useGlobalNamespace: useGlobalNamespace);
        var (invocationExpression, _, _) = FindLoggerMethodInvocation(null, syntaxTree);

        var (ns, className) = LogCallCallerExtractor.Extract(invocationExpression);

        ns.Should().Be(expectedNamespace);
        className.Should().Be(expectedClassName);
    }
    
    [Fact]
    public void Extract_WithNestedClasses_ShouldReturnOuterClassName()
    {
        const string additionalDeclaration = """
                                             class Outer
                                             {
                                                 class Inner 
                                                 {
                                                     private ILogger logger;
                                                     public void Log() => logger.LogInformation("Hello world");
                                                 }
                                             }
                                             """;
        var (_, syntaxTree) = CompileSourceCode(string.Empty, additionalDeclaration);
        var (invocationExpression, _, _) = FindLoggerMethodInvocation(null, syntaxTree);

        var (ns, className) = LogCallCallerExtractor.Extract(invocationExpression);

        ns.Should().Be(Namespace);
        className.Should().Be(ClassName);
    }
}
