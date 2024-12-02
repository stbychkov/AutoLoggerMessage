using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using AutoLoggerMessageGenerator.Import.Microsoft.Extensions.Telemetry.LoggerMessage;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.Utilities;
using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.Extractors;

internal class LogParametersExtractor(
    LogPropertiesCheck? logPropertiesCheck = null, 
    ParameterNameNormalizer? parameterNameNormalizer = null
)
{
    private static readonly Regex MessageArgumentsRegex = new(@"\{(.*?)\}", RegexOptions.Compiled);
    
    public ImmutableArray<LogCallParameter>? Extract(string message, IMethodSymbol methodSymbol)
    {
        var matches = MessageArgumentsRegex.Matches(message);
        var templateParametersNames = matches.OfType<Match>().Select(c => c.Groups[1].Value).ToArray();

        var argumentParameterPrefix = Constants.ArgumentName.TrimStart('@');
        var methodParameters = methodSymbol.Parameters
            .Where(c => c.Name.StartsWith(argumentParameterPrefix))
            .ToArray();

        if (templateParametersNames.Length < methodParameters.Length)
            return null;
        
        var utilityParameters = methodSymbol.Parameters
            .Where(c => Constants.ReservedArgumentNames.Contains($"@{c.Name}"))
            .Select(parameter => CreateLogCallParameter(parameter.Type, parameter.Name, false));

        var messageParameters = methodParameters
            .Select((parameter, ix) => CreateLogCallParameter(
                    type: parameter.Type,
                    name: templateParametersNames[ix],
                    hasPropertiesToLog: logPropertiesCheck?.IsApplicable(parameter.Type) ?? false
                )
            );

        return utilityParameters.Concat(messageParameters).ToImmutableArray();
    }
    
    private LogCallParameter CreateLogCallParameter(ITypeSymbol type, string name, bool hasPropertiesToLog)
    {
        name = parameterNameNormalizer?.Normalize(name) ?? name;

        return new LogCallParameter(
            Type: type.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions
                    .IncludeNullableReferenceTypeModifier)),
            Name: name,
            HasPropertiesToLog: hasPropertiesToLog);
    }
}
