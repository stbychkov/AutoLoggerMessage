using AutoLoggerMessageGenerator.Import.Microsoft.Extensions.Telemetry.LoggerMessage;
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
        var (ns, className) = LogCallCallerExtractor.Extract(invocationExpression);

        var location = LogCallLocationMapper.Map(semanticModel, invocationExpression);
        if (location is null)
            return default;

        var logLevel = LogLevelExtractor.Extract(methodSymbol, invocationExpression);
        if (logLevel is null)
            return default;

        var message = LogCallMessageExtractor.Extract(methodSymbol, invocationExpression, semanticModel);
        if (message is null)
            return default;

        var logPropertiesCheck = new LogPropertiesCheck(semanticModel.Compilation);
        var parameters = new LogCallParametersExtractor(logPropertiesCheck)
            .Extract(message, methodSymbol);

        if (parameters is null)
            return default;

        return new LogCall(Guid.NewGuid(), location.Value, ns, className, methodSymbol.Name, logLevel, message, parameters.Value);
    }
}
