using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Import.Microsoft.Extensions.Telemetry.LoggerMessage;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.Utilities;
using Microsoft.CodeAnalysis;

using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.Extractors;

internal class CallParametersExtractor(LogPropertiesCheck? logPropertiesCheck = null)
{
    public ImmutableArray<CallParameter>? Extract(string message, IMethodSymbol methodSymbol)
    {
        var templateParametersNames = MessageParameterNamesExtractor.Extract(message)
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
                    LoggerParameterName => CallParameterType.Logger,
                    LogLevelParameterName => CallParameterType.LogLevel,
                    ExceptionParameterName => CallParameterType.Exception,
                    EventIdParameterName => CallParameterType.EventId,
                    MessageParameterName => CallParameterType.Message,
                    _ => CallParameterType.None
                };
                return CreateLogCallParameter(parameter.Type, $"{parameterName}{uniqueNameSuffix}", type, false);
            });

        var messageParameters = methodParameters
            .Select((parameter, ix) => CreateLogCallParameter(
                    nativeType: parameter.Type,
                    name: templateParametersNames[ix],
                    type: CallParameterType.Others,
                    hasPropertiesToLog: logPropertiesCheck?.IsApplicable(parameter.Type) ?? false
                )
            );

        return utilityParameters.Concat(messageParameters).ToImmutableArray();
    }

    private static CallParameter CreateLogCallParameter(ITypeSymbol @nativeType, string name, CallParameterType type, bool hasPropertiesToLog) =>
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
