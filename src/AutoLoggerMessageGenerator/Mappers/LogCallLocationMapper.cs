using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.Mappers;

internal static class LogCallLocationMapper
{
    public static LogCallLocation? Map(InvocationExpressionSyntax invocationExpression)
    {
        var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
        if (memberAccessExpression?.Expression is not IdentifierNameSyntax identifierName)
            return null;

        var skipSymbols = identifierName.Identifier.ValueText.Length + 1; // obj accessor + dot symbol

        var location = invocationExpression.GetLocation();
        var lineSpan = location.GetLineSpan();
        var linePositionSpan = location.GetLineSpan().Span;

        return new LogCallLocation(lineSpan.Path,
            lineSpan.StartLinePosition.Line + 1,
            linePositionSpan.Start.Character + skipSymbols + 1,
            Context: location
        );
    }
}
