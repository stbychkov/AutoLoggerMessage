using System.Collections.Immutable;
using System.Text;
using System.Threading;
using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.Filters;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AutoLoggerMessageGenerator.Generators;

[Generator]
public class AutoLoggerMessageGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugHelper.Debug();

        var generatorOptions = GeneratorOptionsProvider.Provide(context);
        
        var logCallsProvider = context.SyntaxProvider.CreateSyntaxProvider(
                LogCallFilter.IsLogCallInvocation,
                static (ctx, cts) => GetLogCalls(ctx, cts)
            )
            .Where(static t => t.HasValue)
            .Select(static (t, _) => t.Value)
            .Collect()
            .WithTrackingName("Searching for log calls");

        var inputSource = context.CompilationProvider.Combine(logCallsProvider);

        context.RegisterImplementationSourceOutput(inputSource,
            static (ctx, t) => GenerateCode(ctx, t.Left, t.Right));

        context.RegisterImplementationSourceOutput(generatorOptions, static (ctx, configuration) =>
        {
            if (configuration.GenerateInterceptorAttribute)
                ctx.AddSource("InterceptorAttribute.g.cs", SourceText.From(InterceptorAttributeEmitter.Emit(), Encoding.UTF8));
        });
    }
    
    private static LogCall? GetLogCalls(GeneratorSyntaxContext context, CancellationToken cts)
    {
        var semanticModel = context.SemanticModel;
        var invocationExpression = (InvocationExpressionSyntax)context.Node;
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpression);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol || 
            !LogCallFilter.IsLoggerMethod(methodSymbol) || 
            cts.IsCancellationRequested)
            return default;

        return LogCallExtractor.Extract(methodSymbol, invocationExpression, semanticModel);
    }
    
    private static void GenerateCode(SourceProductionContext context, Compilation compilation, ImmutableArray<LogCall> logCalls)
    {
        // TODO: Add
        DebugHelper.Debug();
    }
}
