using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoLoggerMessageGenerator.VirtualLoggerMessage;

internal static class VirtualMembersInjector
{
    public static Compilation InjectMembers(Compilation injectTo, MemberDeclarationSyntax memberDeclarationSyntax)
    {
        var compilationUnit = CompilationUnit().AddMembers(memberDeclarationSyntax);

        var csharpOptions = injectTo.SyntaxTrees.First().Options as CSharpParseOptions;
        var syntaxTree = CSharpSyntaxTree.ParseText(compilationUnit.NormalizeWhitespace().ToFullString(), csharpOptions);
        var compilation = injectTo.AddSyntaxTrees(syntaxTree);

        return compilation;
    }
}
