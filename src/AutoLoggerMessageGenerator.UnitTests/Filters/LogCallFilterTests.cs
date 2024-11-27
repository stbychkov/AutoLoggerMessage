using AutoLoggerMessageGenerator.Filters;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.UnitTests.Filters;

public class LogCallFilterTests : BaseSourceGeneratorTest
{
    [Fact]
    public void Filter_WithDifferentMethodInvocations_ShouldFilterOnlyLogMethodsFromLoggerInstance()
    {
        var additionalDeclarations = """
                                     public class AnotherObject
                                     {
                                        public void LogInformation(string message) {}
                                        public void LogInformation2(string message) {}
                                     }
                                     
                                     public void LogInformation(string message) {}
                                     public void LogSomething(string message) {}
                                     public void AnotherMethod(string message) {}
                                     
                                     """;
        var sourceCode = $"""
                         AnotherMethod(default);
                         LogInformation(default);
                         LogSomething(default);
                         new AnotherObject().LogInformation(default);
                         new AnotherObject().LogInformation2(default);
                         {LoggerName}.LogInformation(default);
                         """;
        var (compilation, syntaxTree) = CompileSourceCode(sourceCode, additionalDeclarations);

        var nodes = syntaxTree.GetRoot().DescendantNodes();
        var invocationExpressions = nodes.Where(c => LogCallFilter.IsLogCallInvocation(c, CancellationToken.None))
            .ToArray();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var filteredInvocationExpressions = invocationExpressions.OfType<InvocationExpressionSyntax>().Where(c =>
        {
            var methodSymbol = semanticModel.GetSymbolInfo(c).Symbol as IMethodSymbol;
            return LogCallFilter.IsLoggerMethod(methodSymbol);
        }).ToArray();
        
        invocationExpressions.Length.Should().Be(2);
        filteredInvocationExpressions.Length.Should().Be(1);
    }
}
