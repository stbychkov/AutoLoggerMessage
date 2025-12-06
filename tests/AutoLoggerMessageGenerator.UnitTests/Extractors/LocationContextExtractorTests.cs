using AutoLoggerMessageGenerator.Extractors;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

internal class LocationContextExtractorTests : BaseSourceGeneratorTest
{
    [Test]
    [Arguments(false, Namespace, ClassName, Method)]
    [Arguments(true, "", ClassName, Method)]
    public async Task Extract_WithLogCallAndGivenNamespace_ShouldReturnExpectedNamespaceAndClassName(
        bool useGlobalNamespace, string expectedNamespace, string expectedClassName, string expectedMethodName)
    {
        var message = $$"""{{LoggerName}}.LogInformation("Hello world");""";
        var (_, syntaxTree) = await CompileSourceCode(message, useGlobalNamespace: useGlobalNamespace);
        var (invocationExpression, _, _) = FindMethodInvocation(null, syntaxTree);

        var (ns, className, methodName) = LocationContextExtractor.Extract(invocationExpression);

        await Assert.That(ns).IsEqualTo(expectedNamespace);
        await Assert.That(className).IsEqualTo(expectedClassName);
        await Assert.That(methodName).IsEqualTo(expectedMethodName);
    }

    [Test]
    public async Task Extract_WithNestedClasses_ShouldReturnInnerClassName()
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

        var (ns, className, methodName) = LocationContextExtractor.Extract(invocationExpression);

        await Assert.That(ns).IsEqualTo(Namespace);
        await Assert.That(className).IsEqualTo("Inner");
        await Assert.That(methodName).IsEqualTo("Log");
    }

    [Test]
    public async Task Extract_FromAnonymousFunction_ShouldReturnEmptyMethodName()
    {
        const string additionalDeclaration = """
                                             class Foo
                                             {
                                                 private ILogger logger;
                                                 private Action Bar = () => logger.LogInformation("Hello world");
                                             }
                                             """;
        var (_, syntaxTree) = await CompileSourceCode(string.Empty, additionalDeclaration);
        var (invocationExpression, _, _) = FindMethodInvocation(null, syntaxTree);

        var (ns, className, methodName) = LocationContextExtractor.Extract(invocationExpression);

        await Assert.That(ns).IsEqualTo(Namespace);
        await Assert.That(className).IsEqualTo("Foo");
        await Assert.That(methodName).IsEqualTo(string.Empty);
    }
}
