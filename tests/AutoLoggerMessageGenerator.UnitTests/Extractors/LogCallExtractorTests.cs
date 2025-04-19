using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.UnitTests.Utilities;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

internal class LogCallExtractorTests : BaseSourceGeneratorTest
{
    [Test]
    [Arguments($$"""{{LoggerName}}.LogInformation("Hello world {arg1} {arg2}", 1, true);""", true)]
    [Arguments($$"""{{LoggerName}}.LogInformation(null);""", false)]
    [Arguments($$"""{{LoggerName}}.LogInformation("Hello world {arg1}", 1, true);""", false)]
    public async Task Extract_WithLogMethodInvocationCode_ShouldTransformThemIntoLogCallObject(string sourceCode, bool isValidCall)
    {
        var (compilation, syntaxTree) = await CompileSourceCode(sourceCode);
        var (invocationExpression, methodSymbol, semanticModel) = FindLoggerMethodInvocation(compilation, syntaxTree);

        var logCall = LogCallExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

        if (isValidCall)
        {
            var configuration = InterceptorConfigurationUtilities.GetInterceptorConfiguration();
            await Verify(logCall).UseTextForParameters(configuration);
        }
        else
        {
            await Assert.That(logCall.HasValue).IsFalse();
        }
    }
}
