using System;
using AutoLoggerMessageGenerator.Mappers;
using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoLoggerMessageGenerator.Extractors;

internal static class LogCallExtractor
{
    public static LogCall? Extract(IMethodSymbol methodSymbol, 
        InvocationExpressionSyntax invocationExpression, 
        SemanticModel semanticModel)
    {
        var ns = methodSymbol.ContainingNamespace.Name;
        var className = methodSymbol.ContainingType.Name;
        var location = LogCallLocationMapper.Map(invocationExpression);

        var logLevel = LogLevelExtractor.Extract(methodSymbol, invocationExpression);
        if (logLevel is null)
            return default;

        var message = LogMessageExtractor.Extract(methodSymbol, invocationExpression, semanticModel);
        if (message is null)
            return default;

        var parameters = new LogParametersExtractor().Extract(message, methodSymbol);
        if (parameters is null)
            return default;

        return new LogCall(location, ns, className, logLevel, message, parameters.Value);
    }
}
