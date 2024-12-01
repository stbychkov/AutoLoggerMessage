using System.Diagnostics;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoLoggerMessageGenerator.UnitTests;

public abstract class BaseSourceGeneratorTest
{
    protected const string LoggerName = "logger";
    protected const string Namespace = "Foo";
    protected const string ClassName = "Test";
    
    protected static (CSharpCompilation Compilation, SyntaxTree SyntaxTree) CompileSourceCode(
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
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        syntaxTree = compilation.SyntaxTrees.First();

        var diagnostics = syntaxTree.GetDiagnostics();
        var compilationErrors = diagnostics.Where(c => c.Severity == DiagnosticSeverity.Error).ToArray();

        if (compilationErrors.Any())
        {
            Debugger.Launch();
        }

        compilationErrors.Should().BeEmpty();

        return (compilation, syntaxTree);
    }
}
