using AutoLoggerMessageGenerator.Extractors;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

internal class EnclosingClassExtractorTests : BaseSourceGeneratorTest
{
    [Test]
    [Arguments(false, Namespace, ClassName)]
    [Arguments(true, "", ClassName)]
    public async Task Extract_WithLogCallAndGivenNamespace_ShouldReturnExpectedNamespaceAndClassName(
        bool useGlobalNamespace, string expectedNamespace, string expectedClassName)
    {
        var message = $$"""{{LoggerName}}.LogInformation("Hello world");""";
        var (_, syntaxTree) = await CompileSourceCode(message, useGlobalNamespace: useGlobalNamespace);
        var (invocationExpression, _, _) = FindMethodInvocation(null, syntaxTree);

        var (ns, className) = EnclosingClassExtractor.Extract(invocationExpression);

        await Assert.That(ns).IsEqualTo(expectedNamespace);
        await Assert.That(className).IsEqualTo(expectedClassName);
    }

    [Test]
    public async Task Extract_WithNestedClasses_ShouldReturnOuterClassName()
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
        var (_, syntaxTree) = await CompileSourceCode(string.Empty, additionalDeclaration);
        var (invocationExpression, _, _) = FindMethodInvocation(null, syntaxTree);

        var (ns, className) = EnclosingClassExtractor.Extract(invocationExpression);

        await Assert.That(ns).IsEqualTo(Namespace);
        await Assert.That(className).IsEqualTo(ClassName);
    }
}
