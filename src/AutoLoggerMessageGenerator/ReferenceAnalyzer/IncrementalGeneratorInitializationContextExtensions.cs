using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.ReferenceAnalyzer;

public static class IncrementalGeneratorInitializationContextExtensions
{
    public static IncrementalValuesProvider<MetadataReference> GetMetadataReferencesProvider(this IncrementalGeneratorInitializationContext context)
    {
        var metadataProviderProperty = context.GetType().GetProperty(nameof(context.MetadataReferencesProvider))
                                       ?? throw new Exception($"The property '{nameof(context.MetadataReferencesProvider)}' not found");

        var metadataProvider = metadataProviderProperty.GetValue(context);

        if (metadataProvider is IncrementalValuesProvider<MetadataReference> metadataValuesProvider)
            return metadataValuesProvider;

        if (metadataProvider is IncrementalValueProvider<MetadataReference> metadataValueProvider)
            return metadataValueProvider.SelectMany(static (reference, _) => ImmutableArray.Create(reference));

        throw new Exception($"The '{nameof(context.MetadataReferencesProvider)}' is neither an 'IncrementalValuesProvider<{nameof(MetadataReference)}>' nor an 'IncrementalValueProvider<{nameof(MetadataReference)}>.'");
    }
}
