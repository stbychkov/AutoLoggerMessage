using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.Extractors;

internal static class EnclosingClassExtractor
{
    public static (string Namespace, string ClassName) Extract(InvocationExpressionSyntax invocationExpression)
    {
        string className = string.Empty, ns = string.Empty;

        SyntaxNode syntaxNode = invocationExpression;
        while (syntaxNode.Parent is not null)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                className = classDeclarationSyntax.Identifier.Text;

            if (syntaxNode is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
                ns = namespaceDeclarationSyntax.Name.ToString();

            if (syntaxNode is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclarationSyntax)
                ns = fileScopedNamespaceDeclarationSyntax.Name.ToString();

            syntaxNode = syntaxNode.Parent;
        }

        return (ns, className);
    }
}
