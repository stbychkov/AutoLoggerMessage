using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.Extractors;

internal class LogParametersExtractor
{
    public ImmutableArray<LogCallParameter>? Extract(string message, IMethodSymbol methodSymbol)
    {
        var pattern = @"\{(.*?)\}";
        var matches = Regex.Matches(message, pattern);
        var parameterNames = matches.OfType<Match>().Select(c => c.Groups[1].Value).ToArray();

        var methodParameters = methodSymbol.Parameters
            .Where(c => c.Name.StartsWith(LoggerExtensionsEmitter.ArgumentName))
            .ToArray();

        // TODO: diagnostic
        if (parameterNames.Length != methodParameters.Length)
            return null;

        var parameters = methodParameters
            .Select((parameter, ix) => new LogCallParameter(
                Type: parameter.Type.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions
                        .IncludeNullableReferenceTypeModifier)),
                Name: parameterNames[ix].StartsWith("@") ? parameterNames[ix] : '@' + parameterNames[ix])
            ).ToImmutableArray();

        return parameters;
    }
}
