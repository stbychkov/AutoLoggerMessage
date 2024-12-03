using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using static AutoLoggerMessageGenerator.Emitters.LoggerExtensionsEmitter;

namespace AutoLoggerMessageGenerator.Filters;

internal static class LogCallFilter
{
    private static readonly HashSet<string> LogMethodNames =
    [
        nameof(LoggerExtensions.Log),
        nameof(LoggerExtensions.LogTrace),
        nameof(LoggerExtensions.LogDebug),
        nameof(LoggerExtensions.LogInformation),
        nameof(LoggerExtensions.LogWarning),
        nameof(LoggerExtensions.LogError),
        nameof(LoggerExtensions.LogCritical),
    ];

    public static bool IsLoggerMethod(IMethodSymbol methodSymbol) =>
        methodSymbol is { ContainingType: not null, ReceiverType.Name: "ILogger", ReturnsVoid: true } &&
        methodSymbol.ContainingType.ToDisplayString() is $"{Constants.DefaultLoggingNamespace}.{ClassName}" &&
        methodSymbol.IsExtensionMethod &&
        methodSymbol.Parameters.All(c =>
            c.Type.IsAnonymousType is not true && c.Type.TypeKind is not TypeKind.TypeParameter &&
            RecursivelyCheckTypeAccessibility(c.Type)
        );

    public static bool IsLogCallInvocation(SyntaxNode node, CancellationToken cts) =>
        !node.SyntaxTree.FilePath.EndsWith(".g.cs") &&
        node is InvocationExpressionSyntax { ArgumentList.Arguments.Count: > 0 } invocationExpression &&
        !cts.IsCancellationRequested &&
        invocationExpression.Expression.DescendantNodes()
            .Any(c => c is IdentifierNameSyntax identifierNameSyntax && LogMethodNames.Contains(identifierNameSyntax.Identifier.Text));

    private static bool RecursivelyCheckTypeAccessibility(ITypeSymbol typeSymbol) =>
        typeSymbol.DeclaredAccessibility is Accessibility.Friend or Accessibility.Public or Accessibility.Internal &&
        (typeSymbol.ContainingType is null || RecursivelyCheckTypeAccessibility(typeSymbol.ContainingType));
}
