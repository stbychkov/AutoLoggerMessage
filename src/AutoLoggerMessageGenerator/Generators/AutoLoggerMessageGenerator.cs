using System.Text;
using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AutoLoggerMessageGenerator.Generators;

[Generator]
public class AutoLoggerMessageGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugHelper.Debug();

        var generatorOptions = GeneratorOptionsProvider.Provide(context);

        context.RegisterImplementationSourceOutput(generatorOptions, (ctx, configuration) =>
        {
            if (configuration.GenerateInterceptorAttribute)
                ctx.AddSource("InterceptorAttribute.g.cs", SourceText.From(InterceptorAttributeEmitter.Emit(), Encoding.UTF8));
        });
    }
}
