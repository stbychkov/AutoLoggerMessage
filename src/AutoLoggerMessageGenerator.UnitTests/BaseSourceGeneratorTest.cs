using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.UnitTests;

internal abstract class BaseSourceGeneratorTest
{
    public const string LoggerName = "logger";
    public const string Namespace = "Foo";
    public const string ClassName = "Test";

    protected static async Task<(CSharpCompilation Compilation, SyntaxTree SyntaxTree)> CompileSourceCode(
        string body, string additionalClassMemberDeclarations = "",
        bool useGlobalNamespace = false)
    {
        var sourceCode = $$"""
                           using System;
                           using {{Constants.DefaultLoggingNamespace}};

                           {{(useGlobalNamespace ? string.Empty : $"namespace {Namespace};")}}

                           public class {{ClassName}}(ILogger {{LoggerName}})
                           {
                               {{additionalClassMemberDeclarations}}

                               public void Main()
                               {
                                   {{body}}
                               }
                           }
                           """;

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();

        var loggerAssemblyLocation = Path.Join(AppContext.BaseDirectory, "Microsoft.Extensions.Logging.Abstractions.dll");
        references.Add(MetadataReference.CreateFromFile(loggerAssemblyLocation));

        var compilation = CSharpCompilation.Create("SourceGeneratorTests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        syntaxTree = compilation.SyntaxTrees.First();

        var diagnostics = syntaxTree.GetDiagnostics();
        var compilationErrors = diagnostics.Where(c => c.Severity == DiagnosticSeverity.Error).ToArray();

        if (compilationErrors.Any())
            Debugger.Launch();

        await Assert.That(compilationErrors).IsEmpty();

        return (compilation, syntaxTree);
    }

    protected static (InvocationExpressionSyntax, IMethodSymbol?, SemanticModel?) FindLoggerMethodInvocation(
        Compilation? compilation, SyntaxTree syntaxTree)
    {
        var invocationExpression = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        IMethodSymbol? methodSymbol = null;
        SemanticModel? semanticModel = null;

        if (compilation is not null)
        {
            semanticModel = compilation.GetSemanticModel(syntaxTree);
            methodSymbol = (IMethodSymbol) semanticModel.GetSymbolInfo(invocationExpression).Symbol!;
        }

        return (invocationExpression, methodSymbol, semanticModel);
    }
}
