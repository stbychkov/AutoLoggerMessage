using AutoLoggerMessageGenerator.Extractors;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

public class LogCallExtractorTests : BaseSourceGeneratorTest
{
    [Theory]
    [InlineData($$"""{{LoggerName}}.LogInformation("Hello world {arg1} {arg2}", 1, true);""", true)]
    [InlineData($$"""{{LoggerName}}.LogInformation(null);""", false)]
    [InlineData($$"""{{LoggerName}}.LogInformation("Hello world {arg1}", 1, true);""", false)]
    public async Task Extract_WithLogMethodInvocationCode_ShouldTransformThemIntoLogCallObject(string sourceCode, bool isValidCall)
    {
        var (compilation, syntaxTree) = CompileSourceCode(sourceCode);

        var invocationExpression = (await syntaxTree.GetRootAsync()).DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

        var logCall = LogCallExtractor.Extract(methodSymbol, invocationExpression, semanticModel);

        if (isValidCall)
        {
            await Verify(logCall);
        }
        else
        {
            logCall.HasValue.Should().BeFalse();
        }
    }
}
