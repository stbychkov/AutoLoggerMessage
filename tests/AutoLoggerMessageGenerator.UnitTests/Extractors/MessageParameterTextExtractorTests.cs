using AutoLoggerMessageGenerator.Extractors;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

internal class MessageParameterTextExtractorTests : BaseSourceGeneratorTest
{
     [Test]
     [Arguments("Hello world {1}, {2}!")]
     [Arguments("")]
     public async Task Extract_WithPassingLiteralValues_ShouldReturnExpectedMessage(string expectedMessage)
     {
         var (compilation, syntaxTree) = await CompileSourceCode($"""{LoggerName}.LogInformation("{expectedMessage}");""");
         var (invocationExpression, methodSymbol, semanticModel) = FindMethodInvocation(compilation, syntaxTree);

         var result = MessageParameterTextExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

         await Assert.That(result).IsEqualTo(expectedMessage);
     }

     [Test]
     public async Task Extract_WithPassingConstClassMembers_ShouldReturnExpectedMessage()
     {
         var expectedMessage = "Hello world";
         var (compilation, syntaxTree) = await CompileSourceCode(
             $"{LoggerName}.LogInformation(Message);",
             $"""private const string Message = "{expectedMessage}";"""
         );
         var (invocationExpression, methodSymbol, semanticModel) = FindMethodInvocation(compilation, syntaxTree);

         var result = MessageParameterTextExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

         await Assert.That(result).IsEqualTo(expectedMessage);
     }

     [Test]
     public async Task Extract_WithPassingLocalConstVariable_ShouldReturnExpectedMessage()
     {
         var expectedMessage = "Hello world";
         var (compilation, syntaxTree) = await CompileSourceCode($"""
                                                           const string message = "{expectedMessage}";
                                                           {LoggerName}.LogInformation(message);
                                                           """
         );
         var (invocationExpression, methodSymbol, semanticModel) = FindMethodInvocation(compilation, syntaxTree);

         var result = MessageParameterTextExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

         await Assert.That(result).IsEqualTo(expectedMessage);
     }

     [Test]
     public async Task Extract_WithPassingNullValues_ShouldReturnNull()
     {
         var (compilation, syntaxTree) = await CompileSourceCode($"{LoggerName}.LogInformation(null);");
         var (invocationExpression, methodSymbol, semanticModel) = FindMethodInvocation(compilation, syntaxTree);

         var result = MessageParameterTextExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

         await Assert.That(result).IsNull();
     }

     [Test]
     public async Task Extract_WithBinaryExpressions_ShouldReturnConcatenatedString()
     {
         const string part1 = "Hello ";

         var sourceCode = $"""
                           const string part2 = "world! ";
                           const int parameter = 42;
                           {LoggerName}.LogInformation("{part1}" + part2 + parameter);
                           """;

         var (compilation, syntaxTree) = await CompileSourceCode(sourceCode);
         var (invocationExpression, methodSymbol, semanticModel) = FindMethodInvocation(compilation, syntaxTree);

         var result = MessageParameterTextExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

         await Assert.That(result).IsEqualTo("Hello world! 42");
     }

     [Test]
     public async Task Extract_WithBinaryNonConstantExpressions_ShouldReturnNull()
     {
         const string part1 = "Hello ";

         var sourceCode = $"""
                           string part2 = "world! ";
                           int parameter = 42;
                           {LoggerName}.LogInformation("{part1}" + part2 + parameter);
                           """;

         var (compilation, syntaxTree) = await CompileSourceCode(sourceCode);
         var (invocationExpression, methodSymbol, semanticModel) = FindMethodInvocation(compilation, syntaxTree);

         var result = MessageParameterTextExtractor.Extract(methodSymbol!, invocationExpression, semanticModel!);

         await Assert.That(result).IsNull();
     }
}
