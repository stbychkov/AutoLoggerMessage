using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Extractors;
using AutoLoggerMessageGenerator.Filters;
using AutoLoggerMessageGenerator.Mappers;
using AutoLoggerMessageGenerator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoLoggerMessageGenerator.Analysers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvalidTemplateParameterNameAnalyser : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        id: "ALM001",
        title: "Invalid template parameter name",
        messageFormat: $"Template parameters ({{0}}) have invalid names ({IdentifierHelper.ValidCSharpParameterNameRegex})",
        category: "Naming",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Template parameters in log messages must have valid names."
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                               GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        if (!LogCallFilter.IsLogCallInvocation(invocationExpression, context.CancellationToken))
            return;

        var semanticModel = context.SemanticModel;
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
        if (methodSymbol is null || !LogCallFilter.IsLoggerMethod(methodSymbol))
            return;

        var message = LogCallMessageExtractor.Extract(methodSymbol, invocationExpression, semanticModel);
        if (message is null)
            return;

        var templateParameterNames = LogCallMessageParameterNamesExtractor.Extract(message)
            .Where(parameterName => !IdentifierHelper.IsValidCSharpParameterName(parameterName))
            .ToImmutableArray();

        if (!templateParameterNames.Any())
            return;

        var location = LogCallLocationMapper.Map(semanticModel, invocationExpression);
        if (location is null)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, location.Value.Context, string.Join(", ", templateParameterNames)));
    }
}
