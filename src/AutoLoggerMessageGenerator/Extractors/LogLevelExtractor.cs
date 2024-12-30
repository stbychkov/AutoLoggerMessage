using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.Extractors;

internal static class LogLevelExtractor
{
    public static string? Extract(IMethodSymbol? methodSymbol, InvocationExpressionSyntax? invocationExpression) =>
        methodSymbol is null || invocationExpression is null
            ? null
            : ExtractLogLevelFromMethodName(methodSymbol) ?? ExtractLogLevelFromMethodArgument(invocationExpression);

    private static string? ExtractLogLevelFromMethodName(IMethodSymbol methodSymbol)
    {
        var logLevelInMethodName = methodSymbol.Name.AsSpan(3);
        return logLevelInMethodName.IsEmpty ? default : logLevelInMethodName.ToString();
    }

    private static string? ExtractLogLevelFromMethodArgument(InvocationExpressionSyntax invocationExpression)
    {
        var logLevelArgument = invocationExpression.ArgumentList.Arguments.SingleOrDefault(c =>
            c.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "LogLevel" } });

        if (logLevelArgument is null)
            return null;

        return ((MemberAccessExpressionSyntax) logLevelArgument.Expression).Name.GetText().ToString();
    }
}
