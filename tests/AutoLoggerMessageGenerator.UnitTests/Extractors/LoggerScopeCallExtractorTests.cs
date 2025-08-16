using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.UnitTests.Utilities;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

internal class LoggerScopeCallExtractorTests : BaseSourceGeneratorTest
{
    [Test]
    [Arguments("with parameters", $$"""{{LoggerName}}.BeginScope("Hello world {arg1} {arg2}", 1, true);""", true)]
    [Arguments("with null passed", $"""{LoggerName}.BeginScope(null);""", false)]
    [Arguments("with parameter mismatch", $$"""{{LoggerName}}.BeginScope("Hello world {arg1}", 1, true);""", false)]
    public async Task Extract_WithGivenLoggerScope_ShouldTransformIntoLoggerScopeCallObject(string description, string sourceCode, bool isValidCall)
    {
        var (compilation, syntaxTree) = await CompileSourceCode(sourceCode);
        var (invocationExpression, methodSymbol, semanticModel) = FindMethodInvocation(compilation, syntaxTree);

        var loggerScope = LoggerScopeCallExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

        if (isValidCall)
        {
            var configuration = InterceptorConfigurationUtilities.GetInterceptorConfiguration();
            var roslynVersion = RoslynConfigurationUtilities.GetRoslynVersion();
            await Verify(loggerScope).UseParameters(description, configuration, roslynVersion);
        }
        else
        {
            await Assert.That(loggerScope.HasValue).IsFalse();
        }
    }
}
