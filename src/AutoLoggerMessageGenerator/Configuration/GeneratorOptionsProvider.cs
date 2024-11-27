using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoLoggerMessageGenerator.Configuration;

internal static class GeneratorOptionsProvider
{
    private const string CommonPrefix = $"build_property.{nameof(AutoLoggerMessageGenerator)}";
    private const string GenerateInterceptorAttributeKey = $"{CommonPrefix}_{nameof(Configuration.GenerateInterceptorAttribute)}";
    private const string GenerateSkipEnabledCheckKey = $"{CommonPrefix}_{nameof(Configuration.GenerateSkipEnabledCheck)}";
    private const string GenerateOmitReferenceNameKey = $"{CommonPrefix}_{nameof(Configuration.GenerateOmitReferenceName)}";
    private const string GenerateSkipNullPropertiesKey = $"{CommonPrefix}_{nameof(Configuration.GenerateSkipNullProperties)}";
    private const string GenerateTransitiveKey = $"{CommonPrefix}_{nameof(Configuration.GenerateTransitive)}";

    public static IncrementalValueProvider<Configuration> Provide(IncrementalGeneratorInitializationContext context) =>
        context.AnalyzerConfigOptionsProvider.Select((options, _) => new Configuration(
            GenerateInterceptorAttribute: GetValue(options.GlobalOptions, GenerateInterceptorAttributeKey, true),
            GenerateSkipEnabledCheck: GetValue(options.GlobalOptions, GenerateSkipEnabledCheckKey, false),
            GenerateOmitReferenceName: GetValue(options.GlobalOptions, GenerateOmitReferenceNameKey, false),
            GenerateSkipNullProperties: GetValue(options.GlobalOptions, GenerateSkipNullPropertiesKey, false),
            GenerateTransitive: GetValue(options.GlobalOptions, GenerateTransitiveKey, false)
        ));

    private static bool GetValue(AnalyzerConfigOptions options, string key, bool defaultValue = true) =>
        options.TryGetValue(key, out var value) ? IsFeatureEnabled(value, defaultValue) : defaultValue;

    private static bool IsFeatureEnabled(string value, bool defaultValue) =>
        StringComparer.OrdinalIgnoreCase.Equals("enable", value)
        || StringComparer.OrdinalIgnoreCase.Equals("enabled", value)
        || StringComparer.OrdinalIgnoreCase.Equals("true", value)
        || (bool.TryParse(value, out var boolVal) ? boolVal : defaultValue);
}
