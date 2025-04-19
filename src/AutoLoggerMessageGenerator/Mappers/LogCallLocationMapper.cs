using System.Diagnostics;
using System.Text;
using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.Mappers;

internal static class LogCallLocationMapper
{
    public static LogCallLocation? Map(SemanticModel semanticModel, InvocationExpressionSyntax invocationExpression)
    {
        var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
        if (memberAccessExpression?.Expression is not IdentifierNameSyntax identifierName)
            return null;

        var skipSymbols = identifierName.Identifier.ValueText.Length + 1; // obj accessor + dot symbol

        var location = invocationExpression.GetLocation();
        var lineSpan = location.GetLineSpan();
        var linePositionSpan = location.GetLineSpan().Span;

        var filePath = lineSpan.Path;
        var line = lineSpan.StartLinePosition.Line + 1;
        var character = linePositionSpan.Start.Character + skipSymbols + 1;

#if PATH_BASED_INTERCEPTORS
        var interceptableLocation = GeneratePathBasedInterceptableLocation(filePath, line, character);
#elif HASH_BASED_INTERCEPTORS
        var interceptableLocation = semanticModel.GetInterceptableLocation(invocationExpression)?.GetInterceptsLocationAttributeSyntax();
#else
        throw new NotSupportedException("Unknown interceptors configuration");
#endif

        if (interceptableLocation is null)
            return null;

        return new LogCallLocation(filePath, line, character, interceptableLocation, location);
    }

    #if PATH_BASED_INTERCEPTORS
    private static string GeneratePathBasedInterceptableLocation(string filePath, int line, int character)
    {
        return $"[{Constants.InterceptorNamespace}.{Constants.InterceptorAttributeName}(" +
                    $"filePath: @\"{filePath}\", " +
                    $"line: {line}, " +
                    $"character: {character}" +
                $")]";
    }
    #endif
}
