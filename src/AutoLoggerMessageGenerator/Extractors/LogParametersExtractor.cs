using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Import.Microsoft.Extensions.Telemetry.LoggerMessage;
using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.Extractors;

internal class LogParametersExtractor(LogPropertiesCheck logPropertiesCheck)
{
    public ImmutableArray<LogCallParameter>? Extract(string message, IMethodSymbol methodSymbol)
    {
        var pattern = @"\{(.*?)\}";
        var matches = Regex.Matches(message, pattern);
        var templateParametersNames = matches.OfType<Match>().Select(c => c.Groups[1].Value).ToArray();

        var methodParameters = methodSymbol.Parameters
            .Where(c => c.Name.StartsWith(LoggerExtensionsEmitter.ArgumentName))
            .ToArray();

        if (templateParametersNames.Length < methodParameters.Length)
            return null;

        var parameters = methodParameters
            .Select((parameter, ix) => new LogCallParameter(
                Type: parameter.Type.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions
                        .IncludeNullableReferenceTypeModifier)),
                Name: templateParametersNames[ix].StartsWith("@") ? templateParametersNames[ix] : '@' + templateParametersNames[ix],
                HasPropertiesToLog: logPropertiesCheck.IsApplicable(parameter.Type))
            ).ToImmutableArray();

        return parameters;
    }
}
