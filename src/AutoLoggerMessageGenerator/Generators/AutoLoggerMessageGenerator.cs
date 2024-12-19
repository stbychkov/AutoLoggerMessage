using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using AutoLoggerMessageGenerator.Caching;
using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Diagnostics;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.Filters;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.PostProcessing;
using AutoLoggerMessageGenerator.ReferenceAnalyzer;
using AutoLoggerMessageGenerator.Utilities;
using AutoLoggerMessageGenerator.VirtualLoggerMessage;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging.Generators;
using Microsoft.Gen.Logging.Parsing;

namespace AutoLoggerMessageGenerator.Generators;

[Generator]
public class AutoLoggerMessageGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DebugHelper.Debug();

        var configuration = GeneratorOptionsProvider.Provide(context);

        var modulesProvider = context.GetMetadataReferencesProvider()
            .SelectMany(static (reference, _) => reference.GetModules())
            .Collect()
            .WithTrackingName("Scanning project references");

        var logCallsProvider = context.SyntaxProvider.CreateSyntaxProvider(
                LogCallFilter.IsLogCallInvocation,
                static (ctx, cts) => GetLogCalls(ctx, cts)
            )
            .Where(static t => t.HasValue)
            .Select(static (t, _) => t!.Value)
            .Collect()
            .WithTrackingName("Searching for log calls");

        var inputSource = context.CompilationProvider.Combine(configuration.Combine(modulesProvider.Combine(logCallsProvider)))
            .WithComparer(new InputSourceComparer());

        context.RegisterImplementationSourceOutput(inputSource,
            static (ctx, t) => GenerateCode(ctx, t.Item1, t.Item2.Item1, t.Item2.Item2.Item1, t.Item2.Item2.Item2));

        context.RegisterImplementationSourceOutput(configuration, static (ctx, configuration) =>
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

    private static void GenerateCode(SourceProductionContext context, Compilation compilation,
        SourceGeneratorConfiguration configuration, ImmutableArray<Reference> modules,
        ImmutableArray<LogCall> logCalls)
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

        var diagnosticReporter = new DiagnosticReporter(context, logCalls);

        var loggerMessageCode = useTelemetryExtensions
            ? GenerateNewLoggerMessage(diagnosticReporter, compilation, classDeclarations, context.CancellationToken)
            : GenerateOldLoggerMessage(diagnosticReporter, compilation, classDeclarations, context.CancellationToken);

        if (context.CancellationToken.IsCancellationRequested) return;

        loggerMessageCode = LoggerMessageResultAdjuster.Adjust(loggerMessageCode);
        if (!string.IsNullOrEmpty(loggerMessageCode))
            context.AddSource("LoggerMessage.g.cs", SourceText.From(loggerMessageCode!, Encoding.UTF8));

        context.AddSource("Interceptors.g.cs", SourceText.From(LoggerInterceptorsEmitter.Emit(logCalls), Encoding.UTF8));
    }

    private static string? GenerateNewLoggerMessage(
        DiagnosticReporter diagnosticReporter, Compilation compilation,
        ImmutableHashSet<ClassDeclarationSyntax> classDeclarationSyntaxes, CancellationToken cts)
    {
        var p = new Parser(compilation, diagnosticReporter.Report, cts);
        var logTypes = p.GetLogTypes(classDeclarationSyntaxes);

        if (logTypes.Count <= 0) return null;

        var e = new Microsoft.Gen.Logging.Emission.Emitter();
        return e.Emit(logTypes, cts);
    }

    private static string? GenerateOldLoggerMessage(
        DiagnosticReporter diagnosticReporter, Compilation compilation,
        ImmutableHashSet<ClassDeclarationSyntax> classDeclarationSyntaxes,  CancellationToken cts)
    {
        var loggerMessageParser = new LoggerMessageGenerator.Parser(compilation, diagnosticReporter.Report, cts);
        var logClasses = loggerMessageParser.GetLogClasses(classDeclarationSyntaxes);

        if (logClasses.Count <= 0) return null;

        var loggerMessageEmitter = new LoggerMessageGenerator.Emitter();
        return loggerMessageEmitter.Emit(logClasses, cts);
    }
}
