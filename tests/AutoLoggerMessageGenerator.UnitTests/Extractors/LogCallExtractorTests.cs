using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.UnitTests.Utilities;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

internal class LogCallExtractorTests : BaseSourceGeneratorTest
{
    [Test]
    [Arguments("without parameters", $$"""{{LoggerName}}.LogInformation("Hello world");""", true)]
    [Arguments("with parameters", $$"""{{LoggerName}}.LogInformation("Hello world {arg1} {arg2}", 1, true);""", true)]
    [Arguments("with only null passed", $$"""{{LoggerName}}.LogInformation(null);""", false)]
    [Arguments("with parameter count mismatch", $$"""{{LoggerName}}.LogInformation("Hello world {arg1}", 1, true);""", false)]
    public async Task Extract_WithLogMethodInvocationCode_ShouldTransformThemIntoLogCallObject(string description, string sourceCode, bool isValidCall)
    {
        var (compilation, syntaxTree) = await CompileSourceCode(sourceCode);
        var (invocationExpression, methodSymbol, semanticModel) = FindMethodInvocation(compilation, syntaxTree);

        var logCall = LogCallExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

        if (isValidCall)
        {
            var configuration = InterceptorConfigurationUtilities.GetInterceptorConfiguration();
            await Verify(logCall).UseParameters(description, configuration);
        }
        else
        {
            await Assert.That(logCall.HasValue).IsFalse();
        }
    }
}
