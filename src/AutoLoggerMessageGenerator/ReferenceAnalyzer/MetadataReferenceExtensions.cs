using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.ReferenceAnalyzer;

public static class MetadataReferenceExtensions
{
    public static IEnumerable<Reference> GetModules(this MetadataReference metadataReference)
    {
        if (metadataReference is CompilationReference compilationReference)
        {
            return compilationReference.Compilation.Assembly.Modules
                .Select(m => new Reference(m.Name, compilationReference.Compilation.Assembly.Identity.Version));
        }

        if (metadataReference is PortableExecutableReference portable && portable.GetMetadata() is AssemblyMetadata assemblyMetadata)
        {
            return assemblyMetadata.GetModules()
                .Select(m => new Reference(m.Name, m.GetMetadataReader().GetAssemblyDefinition().Version));
        }

        return Array.Empty<Reference>();
    }
}
