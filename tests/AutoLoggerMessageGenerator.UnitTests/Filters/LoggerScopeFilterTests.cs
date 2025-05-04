using AutoLoggerMessageGenerator.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.UnitTests.Filters;

internal class LoggerScopeFilterTests : BaseSourceGeneratorTest
{
     [Test]
     public async Task Filter_WithDifferentMethodInvocations_ShouldFilterOnlyBeginScopeCallsFromLoggerInstance()
     {
         var additionalDeclarations = """
                                      public class AnotherObject
                                      {
                                         public IDisposable? BeginScope(string message, Guid arg1) {}
                                         public IDisposable? BeginScope2(string message, Guid arg1) {}
                                      }

                                      public IDisposable? BeginScope(string message, Guid arg1) {}
                                      """;
         var sourceCode = $$"""
                          const string message = "CorrelationId: {CorrelationId}";
                          var correlationId = Guid.NewGuid();

                          {{LoggerName}}.BeginScope(message, correlationId);

                          AnotherMethod(message, correlationId);
                          BeginScope(message, correlationId);
                          new AnotherObject().BeginScope(message, correlationId);
                          new AnotherObject().BeginScope2(message, correlationId);
                          """;
         var (compilation, syntaxTree) = await CompileSourceCode(sourceCode, additionalDeclarations);

         var invocationExpressions = syntaxTree.GetRoot()
             .DescendantNodes()
             .Where(c => LoggerScopeFilter.IsLoggerScopeInvocation(c, CancellationToken.None))
             .ToArray();

         var semanticModel = compilation.GetSemanticModel(syntaxTree);
         var filteredInvocationExpressions = invocationExpressions.OfType<InvocationExpressionSyntax>().Where(c =>
         {
             var methodSymbol = (IMethodSymbol) semanticModel.GetSymbolInfo(c).Symbol!;
             return LoggerScopeFilter.IsLoggerScopeMethod(methodSymbol);
         }).ToArray();

         await Assert.That(invocationExpressions.Length).IsEqualTo(2);
         await Assert.That(filteredInvocationExpressions.Length).IsEqualTo(1);
     }

     [Test]
     public async Task Filter_WithInaccessibleClassAsParameter_ShouldExcludeBeginScopeCall()
     {
         const string additionalDeclarations = """
                                               public class A
                                               {
                                                   private class B
                                                   {
                                                       public class C
                                                       {
                                                           public class D {}
                                                       }
                                                   }
                                               }
                                               """;

         const string sourceCode = $$"""
                                   var parameter = new A.B.C.D();
                                   {{LoggerName}}.BeginScope("B class is private, so {this} argument is inaccessible as well", parameter);
                                   """;
         var (compilation, syntaxTree) = await CompileSourceCode(sourceCode, additionalDeclarations);
         var (invocationExpressionSyntax, methodSymbol, _) = FindMethodInvocation(compilation, syntaxTree);

         var isLoggerScopeInvocation = LoggerScopeFilter.IsLoggerScopeInvocation(invocationExpressionSyntax, CancellationToken.None);
         var isLoggerScopeCall = LoggerScopeFilter.IsLoggerScopeMethod(methodSymbol!);

         await Assert.That(isLoggerScopeInvocation).IsTrue();
         await Assert.That(isLoggerScopeCall).IsFalse();
     }

     [Test]
     public async Task Filter_WithOnlyMessageParameterProvided_ShouldExcludeBeginScopeCall()
     {
         const string sourceCode = $"{LoggerName}.BeginScope(\"Some message\");";
         var (compilation, syntaxTree) = await CompileSourceCode(sourceCode);
         var (invocationExpressionSyntax, methodSymbol, _) = FindMethodInvocation(compilation, syntaxTree);

         var isLoggerScopeInvocation = LoggerScopeFilter.IsLoggerScopeInvocation(invocationExpressionSyntax, CancellationToken.None);
         var isLoggerScopeCall = LoggerScopeFilter.IsLoggerScopeMethod(methodSymbol!);

         await Assert.That(isLoggerScopeInvocation).IsFalse();
         await Assert.That(isLoggerScopeCall).IsFalse();
     }
}
