using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.Extractors;

internal static class LogMessageExtractor
{
    public static string? Extract(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocationExpressionSyntax,
        SemanticModel semanticModel)
    {
        var messageParameter = methodSymbol.Parameters.Single(c => c.Name.AsSpan().TrimStart('@') is "message");
        var messageParameterIx = methodSymbol.Parameters.IndexOf(messageParameter);

        var valueExpression = invocationExpressionSyntax.ArgumentList.Arguments[messageParameterIx].Expression;

        if (valueExpression is LiteralExpressionSyntax literalExpression)
            return literalExpression.Token.Value?.ToString();

        if (valueExpression is IdentifierNameSyntax identifierName)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(identifierName);
            var symbol = symbolInfo.Symbol;

            switch (symbol)
            {
                case IFieldSymbol fieldSymbol:
                    return fieldSymbol.ConstantValue?.ToString();
                case ILocalSymbol localSymbol:
                    return localSymbol.ConstantValue?.ToString();
            }
        }

        return null;
    }
}
