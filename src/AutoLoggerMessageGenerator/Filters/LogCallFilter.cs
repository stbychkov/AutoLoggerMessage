using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.Filters;

internal static class LogCallFilter
{
    private static HashSet<string> LogMethodNames =
    [
        // TODO: Support Log method
        // nameof(LoggerExtensions.Log),
        nameof(LoggerExtensions.LogTrace),
        nameof(LoggerExtensions.LogDebug),
        nameof(LoggerExtensions.LogInformation),
        nameof(LoggerExtensions.LogWarning),
        nameof(LoggerExtensions.LogError),
        nameof(LoggerExtensions.LogCritical),
    ];
    
    public static bool IsLoggerMethod(IMethodSymbol methodSymbol)
    {
        var containingType = methodSymbol.ContainingType;
        return containingType != null && methodSymbol.ReceiverType.Name == "ILogger";
    }
    
    public static bool IsLogCallInvocation(SyntaxNode node, CancellationToken cts) =>
        !node.SyntaxTree.FilePath.EndsWith("g.cs") &&
        node is InvocationExpressionSyntax { ArgumentList.Arguments.Count: > 0 } invocationExpression &&
        !cts.IsCancellationRequested &&
        invocationExpression.Expression.DescendantNodes()
            .Any(c => c is IdentifierNameSyntax identifierNameSyntax && LogMethodNames.Contains(identifierNameSyntax.Identifier.Text));
}
