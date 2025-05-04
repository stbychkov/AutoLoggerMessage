using System.Collections.Immutable;
using System.Text;
using AutoLoggerMessageGenerator.Caching;
using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Diagnostics;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.Filters;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.PostProcessing;
using AutoLoggerMessageGenerator.ReferenceAnalyzer;
using AutoLoggerMessageGenerator.VirtualLoggerMessage;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging.Generators;
using Microsoft.Gen.Logging.Parsing;

namespace AutoLoggerMessageGenerator.Generators;

public partial class AutoLoggerMessageGenerator
{
    private static void GenerateLoggerMessages(IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<SourceGeneratorConfiguration> configuration, IncrementalValueProvider<ImmutableArray<Reference>> modulesProvider)
    {
        var logCallsProvider = context.SyntaxProvider.CreateSyntaxProvider(
                LogMessageCallFilter.IsLogCallInvocation,
                static (ctx, cts) => ParseLogCall(ctx, cts)
            )
            .Where(static t => t.HasValue)
            .Select(static (t, _) => t!.Value)
            .Collect()
            .WithTrackingName("Searching for log calls");

        var inputSource = context.CompilationProvider.Combine(configuration.Combine(modulesProvider.Combine(logCallsProvider)))
            .WithComparer(new LogCallInputSourceComparer());

        context.RegisterImplementationSourceOutput(inputSource,
            static (ctx, t) => GenerateCode(ctx, t.Item1, t.Item2.Item1, t.Item2.Item2.Item1, t.Item2.Item2.Item2));
    }

    private static LogMessageCall? ParseLogCall(GeneratorSyntaxContext context, CancellationToken cts)
    {
        var semanticModel = context.SemanticModel;
        var invocationExpression = (InvocationExpressionSyntax)context.Node;
        var symbolInfo = semanticModel.GetSymbolInfo(invocationExpression);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol ||
            !LogMessageCallFilter.IsLoggerMethod(methodSymbol) ||
            cts.IsCancellationRequested)
            return default;

        return LogCallExtractor.Extract(methodSymbol, invocationExpression, semanticModel);
    }

    private static void GenerateCode(SourceProductionContext context, Compilation compilation,
        SourceGeneratorConfiguration configuration, ImmutableArray<Reference> modules,
        ImmutableArray<LogMessageCall> logCalls)
    {
        if (logCalls.IsDefaultOrEmpty || context.CancellationToken.IsCancellationRequested) return;

        var telemetryAbstractions = "Microsoft.Extensions.Telemetry.Abstractions.dll";
        var useTelemetryExtensions = modules.Any(c => c.Name == telemetryAbstractions);

        // We need to keep the current LoggerMessage source generator as it makes future updates easier.
        // All [LoggerMessage] attributes are generated to match the current implementation.
        // These attributes are only used to trigger the source generator, so we can create them virtually and add them to the compilation unit.
        var virtualClassDeclaration = new VirtualLoggerMessageClassBuilder(configuration, useTelemetryExtensions)
            .Build(logCalls);
        compilation = VirtualMembersInjector.InjectMembers(compilation, virtualClassDeclaration);

        var classDeclarations = compilation.SyntaxTrees.Last()
            .GetRoot()
            .DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .ToImmutableHashSet();

        var diagnosticReporter = new LogMessageDiagnosticReporter(context, logCalls);

        var loggerMessageCode = useTelemetryExtensions
            ? GenerateNewLoggerMessage(diagnosticReporter, compilation, classDeclarations, context.CancellationToken)
            : GenerateOldLoggerMessage(diagnosticReporter, compilation, classDeclarations, context.CancellationToken);

        if (context.CancellationToken.IsCancellationRequested) return;

        loggerMessageCode = LoggerMessageResultAdjuster.Adjust(loggerMessageCode);
        if (!string.IsNullOrEmpty(loggerMessageCode))
            context.AddSource($"{Constants.LoggerMessageGeneratorName}.g.cs", SourceText.From(loggerMessageCode!, Encoding.UTF8));

        context.AddSource("LogCallInterceptors.g.cs", SourceText.From(LoggerInterceptorsEmitter.Emit(logCalls), Encoding.UTF8));
    }

    private static string? GenerateNewLoggerMessage(
        LogMessageDiagnosticReporter diagnosticReporter, Compilation compilation,
        ImmutableHashSet<ClassDeclarationSyntax> classDeclarationSyntaxes, CancellationToken cts)
    {
        var p = new Parser(compilation, diagnosticReporter.Report, cts);
        var logTypes = p.GetLogTypes(classDeclarationSyntaxes);

        if (logTypes.Count <= 0) return null;

        var e = new Microsoft.Gen.Logging.Emission.Emitter();
        return e.Emit(logTypes, cts);
    }

    private static string? GenerateOldLoggerMessage(
        LogMessageDiagnosticReporter diagnosticReporter, Compilation compilation,
        ImmutableHashSet<ClassDeclarationSyntax> classDeclarationSyntaxes,  CancellationToken cts)
    {
        var loggerMessageParser = new LoggerMessageGenerator.Parser(compilation, diagnosticReporter.Report, cts);
        var logClasses = loggerMessageParser.GetLogClasses(classDeclarationSyntaxes);

        if (logClasses.Count <= 0) return null;

        var loggerMessageEmitter = new LoggerMessageGenerator.Emitter();
        return loggerMessageEmitter.Emit(logClasses, cts);
    }
}
