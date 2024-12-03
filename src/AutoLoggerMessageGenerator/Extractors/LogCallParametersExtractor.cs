using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using AutoLoggerMessageGenerator.Import.Microsoft.Extensions.Telemetry.LoggerMessage;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.Utilities;
using Microsoft.CodeAnalysis;

using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.Extractors;

internal partial class LogCallParametersExtractor(LogPropertiesCheck? logPropertiesCheck = null)
{
    public ImmutableArray<LogCallParameter>? Extract(string message, IMethodSymbol methodSymbol)
    {
        var matches = MessageArgumentRegex().Matches(message);
        var templateParametersNames = matches.Select(c => c.Groups[1].Value)
            .Select(TransformParameterName)
            .ToArray();

        var argumentParameterPrefix = ParameterName.TrimStart('@');
        var methodParameters = methodSymbol.Parameters
            .Where(c => c.Name.StartsWith(argumentParameterPrefix))
            .ToArray();

        if (templateParametersNames.Length < methodParameters.Length)
            return null;

        var uniqueNameSuffix = ReservedParameterNameResolver.GenerateUniqueIdentifierSuffix(templateParametersNames);

        var utilityParameters = methodSymbol.Parameters
            .Where(c => ReservedParameterNames.Contains($"@{c.Name}"))
            .Select(parameter =>
            {
                var parameterName = TransformParameterName(parameter.Name);
                var type = parameterName switch
                {
                    LoggerParameterName => LogCallParameterType.Logger,
                    LogLevelParameterName => LogCallParameterType.LogLevel,
                    ExceptionParameterName => LogCallParameterType.Exception,
                    EventIdParameterName => LogCallParameterType.EventId,
                    MessageParameterName => LogCallParameterType.Message,
                    _ => LogCallParameterType.None
                };
                return CreateLogCallParameter(parameter.Type, $"{parameterName}{uniqueNameSuffix}", type, false);
            });

        var messageParameters = methodParameters
            .Select((parameter, ix) => CreateLogCallParameter(
                    nativeType: parameter.Type,
                    name: templateParametersNames[ix],
                    type: LogCallParameterType.Others,
                    hasPropertiesToLog: logPropertiesCheck?.IsApplicable(parameter.Type) ?? false
                )
            );

        return utilityParameters.Concat(messageParameters).ToImmutableArray();
    }

    private static LogCallParameter CreateLogCallParameter(ITypeSymbol @nativeType, string name,
        LogCallParameterType type, bool hasPropertiesToLog) =>
        new(
            NativeType: nativeType.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions
                    .IncludeNullableReferenceTypeModifier)),
            Name: name,
            Type: type,
            HasPropertiesToLog: hasPropertiesToLog);

    private static string TransformParameterName(string parameterName) =>
        parameterName.StartsWith('@') ? parameterName : '@' + parameterName;

    [GeneratedRegex(@"\{(.*?)\}", RegexOptions.Compiled)]
    private static partial Regex MessageArgumentRegex();
}
