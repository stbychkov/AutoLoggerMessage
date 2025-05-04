using AutoLoggerMessageGenerator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static AutoLoggerMessageGenerator.Emitters.GenericLoggerScopeExtensionsEmitter;

namespace AutoLoggerMessageGenerator.Filters;

internal static class LoggerScopeFilter
{
    public static bool IsLoggerScopeMethod(IMethodSymbol methodSymbol) =>
        methodSymbol is { ContainingType: not null, ReceiverType.Name: "ILogger", ReturnsVoid: false, ReturnType.Name: "IDisposable" } &&
        methodSymbol.ContainingType.ToDisplayString() is $"{Constants.DefaultLoggingNamespace}.{ClassName}" &&
        methodSymbol.IsExtensionMethod &&
        methodSymbol.Parameters.All(c =>
            c.Type.IsAnonymousType is not true && c.Type.TypeKind is not TypeKind.TypeParameter &&
            TypeAccessibilityChecker.IsAccessible(c.Type)
        );

    public static bool IsLoggerScopeInvocation(SyntaxNode node, CancellationToken cts) =>
        !node.SyntaxTree.FilePath.EndsWith(".g.cs") &&
        node is InvocationExpressionSyntax { ArgumentList.Arguments.Count: > 1 } invocationExpression &&
        !cts.IsCancellationRequested &&
        invocationExpression.Expression.DescendantNodes()
            .Any(c => c is IdentifierNameSyntax { Identifier.Text: "BeginScope" });
}
