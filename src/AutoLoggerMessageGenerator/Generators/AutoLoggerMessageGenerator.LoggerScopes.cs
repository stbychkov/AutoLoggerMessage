using System.Collections.Immutable;
using System.Text;
using AutoLoggerMessageGenerator.Caching;
using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.Filters;
using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AutoLoggerMessageGenerator.Generators;

public partial class AutoLoggerMessageGenerator
{
    private static void GenerateLoggerScopes(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<SourceGeneratorConfiguration> configuration)
    {
        var loggerScopeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                LoggerScopeFilter.IsLoggerScopeInvocation,
                static (ctx, cts) => ParseLoggerScope(ctx, cts)
            )
            .Where(static t => t.HasValue)
            .Select(static (t, _) => t!.Value)
            .Collect()
            .WithTrackingName("Searching for log scope definitions");

        var inputSource = context.CompilationProvider.Combine(configuration.Combine(loggerScopeProvider))
            .WithComparer(new LogScopeDefinitionInputSourceComparer());

        context.RegisterImplementationSourceOutput(inputSource, static (ctx, t) =>
            GenerateCode(ctx, t.Item2.Item1, t.Item2.Item2));
    }

    private static LoggerScopeCall? ParseLoggerScope(GeneratorSyntaxContext context, CancellationToken cts)
    {
        var semanticModel = context.SemanticModel;
        var invocationExpression = (InvocationExpressionSyntax)context.Node;
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpression);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol ||
            !LoggerScopeFilter.IsLoggerScopeMethod(methodSymbol) ||
            cts.IsCancellationRequested)
            return default;

        return LoggerScopeCallExtractor.Extract(methodSymbol, invocationExpression, semanticModel);
    }

    private static void GenerateCode(SourceProductionContext context,
        SourceGeneratorConfiguration configuration, ImmutableArray<LoggerScopeCall> loggerScopes)
    {
        if (context.CancellationToken.IsCancellationRequested ||
            loggerScopes.IsDefaultOrEmpty ||
            !configuration.OverrideBeginScopeBehavior)
            return;

        var generatedLoggerScopes = LoggerScopesEmitter.Emit(loggerScopes);

        if (!string.IsNullOrEmpty(generatedLoggerScopes))
            context.AddSource($"{Constants.LoggerScopesGeneratorName}.g.cs", SourceText.From(generatedLoggerScopes, Encoding.UTF8));

        context.AddSource("LoggerScopeInterceptors.g.cs", SourceText.From(LoggerScopeInterceptorsEmitter.Emit(loggerScopes), Encoding.UTF8));
    }
}
