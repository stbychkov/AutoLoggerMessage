using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoLoggerMessageGenerator.Configuration;

internal static class GeneratorOptionsProvider
{
    private const string CommonPrefix = $"build_property.{nameof(AutoLoggerMessageGenerator)}";
    private const string GenerateInterceptorAttributeKey = $"{CommonPrefix}_{nameof(Configuration.GenerateInterceptorAttribute)}";

    public static IncrementalValueProvider<Configuration> Provide(IncrementalGeneratorInitializationContext context) =>
        context.AnalyzerConfigOptionsProvider.Select((options, _) => new Configuration(
            GenerateInterceptorAttribute: GetValue(options.GlobalOptions, GenerateInterceptorAttributeKey, true)
        ));

    private static bool GetValue(AnalyzerConfigOptions options, string key, bool defaultValue = true) =>
        options.TryGetValue(key, out var value) ? IsFeatureEnabled(value, defaultValue) : defaultValue;

    private static bool IsFeatureEnabled(string value, bool defaultValue) =>
        StringComparer.OrdinalIgnoreCase.Equals("enable", value)
        || StringComparer.OrdinalIgnoreCase.Equals("enabled", value)
        || StringComparer.OrdinalIgnoreCase.Equals("true", value)
        || (bool.TryParse(value, out var boolVal) ? boolVal : defaultValue);
}
