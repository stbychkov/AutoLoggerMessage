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

        context.RegisterSyntaxNodeAction(AnalyzeLoggerMessageNode, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeLoggerScopeNode, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeLoggerMessageNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        if (!LogMessageCallFilter.IsLogCallInvocation(invocationExpression, context.CancellationToken))
            return;

        var semanticModel = context.SemanticModel;
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
        if (methodSymbol is null || !LogMessageCallFilter.IsLoggerMethod(methodSymbol))
            return;

        AnalyzeNode(context, methodSymbol, invocationExpression, semanticModel);
    }

    private static void AnalyzeLoggerScopeNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        if (!LoggerScopeFilter.IsLoggerScopeInvocation(invocationExpression, context.CancellationToken))
            return;

        var semanticModel = context.SemanticModel;
        var methodSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
        if (methodSymbol is null || !LoggerScopeFilter.IsLoggerScopeMethod(methodSymbol))
            return;

        AnalyzeNode(context, methodSymbol, invocationExpression, semanticModel);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context, IMethodSymbol methodSymbol,
        InvocationExpressionSyntax invocationExpression, SemanticModel semanticModel)
    {
        var message = MessageParameterTextExtractor.Extract(methodSymbol, invocationExpression, semanticModel);
        if (message is null)
            return;

        var templateParameterNames = MessageParameterNamesExtractor.Extract(message)
            .Where(parameterName => !IdentifierHelper.IsValidCSharpParameterName(parameterName))
            .ToImmutableArray();

        if (!templateParameterNames.Any())
            return;

        var location = CallLocationMapper.Map(semanticModel, invocationExpression);
        if (location is null)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rule, location.Value.Context, string.Join(", ", templateParameterNames)));
    }
}
