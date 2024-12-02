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
        var messageParameter = methodSymbol.Parameters.Single(c => c.Name == Constants.MessageArgumentName.TrimStart('@'));
        var messageParameterIx = methodSymbol.Parameters.IndexOf(messageParameter);

        var valueExpression = invocationExpressionSyntax.ArgumentList.Arguments[messageParameterIx].Expression;
        return ResolveValueExpression(valueExpression, semanticModel);
    }

    private static string? ResolveBinaryExpressions(BinaryExpressionSyntax binaryExpressionSyntax, SemanticModel semanticModel)
    {
        var leftBinaryExpressionValue = ResolveValueExpression(binaryExpressionSyntax.Left, semanticModel);
        if (leftBinaryExpressionValue is null) return null;

        var rightBinaryExpressionValue  = ResolveValueExpression(binaryExpressionSyntax.Right, semanticModel);
        if (rightBinaryExpressionValue is null) return null;

        return string.Concat(leftBinaryExpressionValue, rightBinaryExpressionValue);
    }

    private static string? ResolveValueExpression(ExpressionSyntax valueExpression, SemanticModel semanticModel)
    {
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

        if (valueExpression is BinaryExpressionSyntax binaryExpressionSyntax)
            return ResolveBinaryExpressions(binaryExpressionSyntax, semanticModel);

        return null;
    }
}
