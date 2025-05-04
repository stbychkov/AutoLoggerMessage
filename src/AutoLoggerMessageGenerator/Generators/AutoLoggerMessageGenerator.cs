using System.Text;
using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.ReferenceAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AutoLoggerMessageGenerator.Generators;

[Generator]
public partial class AutoLoggerMessageGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var configuration = GeneratorOptionsProvider.Provide(context);

        var modulesProvider = context.GetMetadataReferencesProvider()
            .SelectMany(static (reference, _) => reference.GetModules())
            .Collect()
            .WithTrackingName("Scanning project references");

        GenerateLoggerMessages(context, configuration, modulesProvider);
        GenerateLoggerScopes(context, configuration);
        GenerateInterceptorAttribute(context, configuration);
    }

    private static void GenerateInterceptorAttribute(IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<SourceGeneratorConfiguration> configuration)
    {
        context.RegisterImplementationSourceOutput(configuration, static (ctx, configuration) =>
        {
            if (configuration.GenerateInterceptorAttribute)
                ctx.AddSource("InterceptorAttribute.g.cs", SourceText.From(InterceptorAttributeEmitter.Emit(), Encoding.UTF8));
        });
    }
}
