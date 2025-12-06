using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.Extractors;

internal static class LocationContextExtractor
{
    public static LocationContext Extract(InvocationExpressionSyntax invocationExpression)
    {
        string? className = null, ns = null, methodName = null;

        SyntaxNode syntaxNode = invocationExpression;
        while (syntaxNode.Parent is not null)
        {
            if (className is null && syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                className = classDeclarationSyntax.Identifier.Text;

            if (ns is null)
            {
                if (syntaxNode is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
                    ns = namespaceDeclarationSyntax.Name.ToString();

                if (syntaxNode is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
                    ns = fileScopedNamespaceDeclarationSyntax.Name.ToString();
            }

            if (methodName is null && syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)
                methodName = methodDeclarationSyntax.Identifier.Text;

            syntaxNode = syntaxNode.Parent;
        }

        return new LocationContext(
            ns ?? string.Empty, className ?? string.Empty, methodName ?? string.Empty
        );
    }

    public record LocationContext(string Namespace, string ClassName, string MethodName);
}
