using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Import.Microsoft.Extensions.Telemetry.LoggerMessage;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.Utilities;
using Microsoft.CodeAnalysis;

using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.Extractors;

internal class LogCallParametersExtractor(LogPropertiesCheck? logPropertiesCheck = null)
{
    public ImmutableArray<LogCallParameter>? Extract(string message, IMethodSymbol methodSymbol)
    {
        var templateParametersNames = LogCallMessageParameterNamesExtractor.Extract(message)
            .Select(IdentifierHelper.AddAtPrefixIfNotExists)
            .ToArray();

        if (!templateParametersNames.All(IdentifierHelper.IsValidCSharpParameterName))
            return null;

        var argumentParameterPrefix = ParameterName.TrimStart('@');
        var methodParameters = methodSymbol.Parameters
            .Where(c => c.Name.StartsWith(argumentParameterPrefix))
            .ToArray();

        if (templateParametersNames.Length < methodParameters.Length)
            return null;

        // https://github.com/dotnet/extensions/blob/ca2fe808b3d6c55817467f46ca58657456b4a928/docs/list-of-diagnostics.md?plain=1#L66C4-L66C13
        if (methodParameters.Any(IsGenericParameter))
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
        parameterName.StartsWith("@") ? parameterName : '@' + parameterName;

    private static bool IsGenericParameter(IParameterSymbol parameterSymbol) =>
        parameterSymbol.Type is INamedTypeSymbol { IsGenericType: true } or ITypeParameterSymbol;
}
