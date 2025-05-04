using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

internal class CallParametersExtractorTests : BaseSourceGeneratorTest
{
     [Test]
     public async Task Extract_WithGivenMessageAndParameters_ShouldReturnMessageAndParsedParameters()
     {
         const string message = "The {EventName} was processed in {Time}ms";
         const string parameters = $"""
                                    "{message}", "OrderReceived", 40
                                    """;

         string extensionDeclaration = $$"""
                                         private static void Log<T1, T2>(
                                             string {{MessageParameterName}},
                                             T1 {{ParameterName}}1,
                                             T2 {{ParameterName}}2) {}
                                         """;

         var (compilation, syntaxTree) = await CompileSourceCode($"Log({parameters});", extensionDeclaration);
         var (_, methodSymbol, _) = FindMethodInvocation(compilation, syntaxTree);

         var sut = new CallParametersExtractor();

         var result = sut.Extract(message, methodSymbol!);

         await Assert.That(result).IsEquivalentTo(new CallParameter[]
         {
             new("global::System.String", MessageParameterName, CallParameterType.Message),
             new("global::System.String", "@EventName", CallParameterType.Others),
             new("global::System.Int32", "@Time", CallParameterType.Others),
         });
     }

     [Test]
     public async Task Extract_WithGivenMessageAndNoParameters_ShouldReturnOnlyMessageParameter()
     {
         const string message = "Hello world!";

         string extensionDeclaration = $"private static void Log(string {MessageParameterName}) {{}}";

         var (compilation, syntaxTree) = await CompileSourceCode($"""Log("{message}");""", extensionDeclaration);
         var (_, methodSymbol, _) = FindMethodInvocation(compilation, syntaxTree);

         var sut = new CallParametersExtractor();

         var result = sut.Extract(message, methodSymbol!);

         await Assert.That(result.HasValue).IsTrue();
         await Assert.That(result!.Value.Length).IsEqualTo(1);
         await Assert.That(result.Value[0]).IsEquivalentTo(
             new CallParameter(
                 "global::System.String",
                 MessageParameterName,
                 CallParameterType.Message
             )
         );
     }

     [Test]
     public async Task Extract_WithUtilityParameters_ShouldExtractAllParameters()
     {
         const string message = "The {EventName} was processed in {Time}ms";

         string extensionDeclaration = $$"""
                                         private static void Log<T1, T2>(
                                             LogLevel {{LogLevelParameterName}},
                                             EventId {{EventIdParameterName}},
                                             Exception {{ExceptionParameterName}},
                                             string {{MessageParameterName}},
                                             T1 {{ParameterName}}1,
                                             T2 {{ParameterName}}2) {}
                                         """;

         var (compilation, syntaxTree) = await CompileSourceCode("""
                                                           Log(
                                                               LogLevel.Information,
                                                               new EventId(),
                                                               new Exception(),
                                                               "The {EventName} was processed in {Time}ms",
                                                               "OrderReceived",
                                                               40
                                                           );
                                                           """, extensionDeclaration);
         var (_, methodSymbol, _) = FindMethodInvocation(compilation, syntaxTree);

         var sut = new CallParametersExtractor();

         var result = sut.Extract(message, methodSymbol!);

         await Assert.That(result).IsEquivalentTo(new CallParameter[]
         {
             new("global::Microsoft.Extensions.Logging.LogLevel", LogLevelParameterName, CallParameterType.LogLevel),
             new("global::Microsoft.Extensions.Logging.EventId", EventIdParameterName, CallParameterType.EventId),
             new("global::System.Exception", ExceptionParameterName, CallParameterType.Exception),
             new("global::System.String", MessageParameterName, CallParameterType.Message),
             new("global::System.String", "@EventName", CallParameterType.Others),
             new("global::System.Int32", "@Time", CallParameterType.Others),
         });
     }

     [Test]
     public async Task Extract_WithUsingReservedParameterNamesInTemplate_ShouldTransformReservedTemplateParameterNamesToMakeThemUnique()
     {
         const string message = "{@eventId_} The {message} was processed in {time}ms";
         const string parameters = $"""
                                    new EventId(), "{message}", 1, "OrderReceived", 40
                                    """;

         const string extensionDeclaration = $$"""
                                               private static void Log<T1, T2, T3>(
                                                   EventId {{EventIdParameterName}},
                                                   string {{MessageParameterName}},
                                                   T1 {{ParameterName}}1,
                                                   T2 {{ParameterName}}2,
                                                   T3 {{ParameterName}}3) {}
                                               """;

         var (compilation, syntaxTree) = await CompileSourceCode($"Log({parameters});", extensionDeclaration);
         var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

         var semanticModel = compilation.GetSemanticModel(syntaxTree);
         var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(invocationExpression).Symbol!;

         var sut = new CallParametersExtractor();

         var result = sut.Extract(message, methodSymbol);

         await Assert.That(result).IsEquivalentTo(new CallParameter[]
         {
             new("global::Microsoft.Extensions.Logging.EventId", "@eventId__", CallParameterType.EventId),
             new("global::System.String", "@message__", CallParameterType.Message),
             new("global::System.Int32", "@eventId_", CallParameterType.Others),
             new("global::System.String", "@message", CallParameterType.Others),
             new("global::System.Int32", "@time", CallParameterType.Others),
         });
     }

     [Test]
     [Arguments("T")]
     [Arguments("List<T>")]
     public async Task Extract_WithGenericParameters_ShouldSkipExtractingParameters(string genericType)
     {
         var message = "{GenericParameter}";
         var (compilation, syntaxTree) = await CompileSourceCode(string.Empty,
    $$"""
                private static void Log<T>(string {{MessageParameterName}}, T {{ParameterName}}) {}

                public void Foo<T>({{genericType}} arg)
                {
                    Log<{{genericType}}>("{{message}}", arg);
                }
            """);
         var (_, methodSymbol, _) = FindMethodInvocation(compilation, syntaxTree);

         var sut = new CallParametersExtractor();

         var result = sut.Extract(message, methodSymbol!);

         await Assert.That(result).IsNull();
     }
}

