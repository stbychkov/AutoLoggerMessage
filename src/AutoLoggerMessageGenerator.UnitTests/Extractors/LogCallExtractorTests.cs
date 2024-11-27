using AutoLoggerMessageGenerator.Extractors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

public class LogCallExtractorTests : BaseSourceGeneratorTest
{
    [Fact]
    public async Task Extract_WithLogMethodInvocationCode_ShouldTransformThemIntoLogCallObject()
    {
        var sourceCode = $"GenericLoggerExtensions.LogWarning({LoggerName}, \"Hello world {{arg1}} {{arg2}}\", 1, true);";
        var (compilation, syntaxTree) = CompileSourceCode(sourceCode);
        
        var invocationExpression = (await syntaxTree.GetRootAsync()).DescendantNodes()
            .Where(c => c is InvocationExpressionSyntax)
            .Cast<InvocationExpressionSyntax>()
            .Single();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

        var logCall = LogCallExtractor.Extract(methodSymbol, invocationExpression, semanticModel);

        await Verify(logCall);
    }
}
