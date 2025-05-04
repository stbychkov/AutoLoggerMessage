using AutoLoggerMessageGenerator.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.UnitTests.Filters;

internal class LogMessageCallFilterTests : BaseSourceGeneratorTest
{
     [Test]
     public async Task Filter_WithDifferentMethodInvocations_ShouldFilterOnlyLogMethodsFromLoggerInstance()
     {
         var additionalDeclarations = """
                                      public class AnotherObject
                                      {
                                         public void LogInformation(string message, DateTime arg1) {}
                                         public void LogInformation2(string message, DateTime arg1) {}
                                      }

                                      public void LogInformation(string message, DateTime arg1) {}
                                      public void LogSomething(string message, DateTime arg1) {}
                                      public void AnotherMethod(string message, DateTime arg1) {}

                                      """;
         var sourceCode = $$"""
                          const string message = "Event received at: {EventTime}";
                          var eventTime = DateTime.Now;

                          {{LoggerName}}.LogInformation(message, eventTime);

                          AnotherMethod(message, eventTime);
                          LogInformation(message, eventTime);
                          LogSomething(message, eventTime);
                          new AnotherObject().LogInformation(message, eventTime);
                          new AnotherObject().LogInformation2(message, eventTime);
                          """;
         var (compilation, syntaxTree) = await CompileSourceCode(sourceCode, additionalDeclarations);

         var invocationExpressions = syntaxTree.GetRoot()
             .DescendantNodes()
             .Where(c => LogMessageCallFilter.IsLogCallInvocation(c, CancellationToken.None))
             .ToArray();

         var semanticModel = compilation.GetSemanticModel(syntaxTree);
         var filteredInvocationExpressions = invocationExpressions.OfType<InvocationExpressionSyntax>().Where(c =>
         {
             var methodSymbol = (IMethodSymbol) semanticModel.GetSymbolInfo(c).Symbol!;
             return LogMessageCallFilter.IsLoggerMethod(methodSymbol);
         }).ToArray();

         await Assert.That(invocationExpressions.Length).IsEqualTo(2);
         await Assert.That(filteredInvocationExpressions.Length).IsEqualTo(1);
     }


     [Test]
     public async Task Filter_WithInaccessibleClassAsParameter_ShouldExcludeLogCall()
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
                                   {{LoggerName}}.LogInformation("B class is private, so {this} argument is inaccessible as well", parameter);
                                   """;
         var (compilation, syntaxTree) = await CompileSourceCode(sourceCode, additionalDeclarations);
         var (invocationExpressionSyntax, methodSymbol, _) = FindMethodInvocation(compilation, syntaxTree);

         var isLogCallInvocation = LogMessageCallFilter.IsLogCallInvocation(invocationExpressionSyntax, CancellationToken.None);
         var isLogCallMethod = LogMessageCallFilter.IsLoggerMethod(methodSymbol!);

         await Assert.That(isLogCallInvocation).IsTrue();
         await Assert.That(isLogCallMethod).IsFalse();
     }
}
