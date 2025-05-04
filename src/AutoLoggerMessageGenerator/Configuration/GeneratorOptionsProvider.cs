using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoLoggerMessageGenerator.Configuration;

internal static class GeneratorOptionsProvider
{
    private const string CommonPrefix = $"build_property.{nameof(AutoLoggerMessageGenerator)}";
    private const string GenerateInterceptorAttributeKey = $"{CommonPrefix}_{nameof(SourceGeneratorConfiguration.GenerateInterceptorAttribute)}";
    private const string GenerateSkipEnabledCheckKey = $"{CommonPrefix}_{nameof(SourceGeneratorConfiguration.GenerateSkipEnabledCheck)}";
    private const string GenerateOmitReferenceNameKey = $"{CommonPrefix}_{nameof(SourceGeneratorConfiguration.GenerateOmitReferenceName)}";
    private const string GenerateSkipNullPropertiesKey = $"{CommonPrefix}_{nameof(SourceGeneratorConfiguration.GenerateSkipNullProperties)}";
    private const string GenerateTransitiveKey = $"{CommonPrefix}_{nameof(SourceGeneratorConfiguration.GenerateTransitive)}";
    private const string OverrideBeginScopeBehavior = $"{CommonPrefix}_{nameof(SourceGeneratorConfiguration.OverrideBeginScopeBehavior)}";

    public static IncrementalValueProvider<SourceGeneratorConfiguration> Provide(IncrementalGeneratorInitializationContext context) =>
        context.AnalyzerConfigOptionsProvider.Select((options, _) => new SourceGeneratorConfiguration(
            GenerateInterceptorAttribute: GetValue(options.GlobalOptions, GenerateInterceptorAttributeKey, true),
            GenerateSkipEnabledCheck: GetValue(options.GlobalOptions, GenerateSkipEnabledCheckKey, true),
            GenerateOmitReferenceName: GetValue(options.GlobalOptions, GenerateOmitReferenceNameKey, false),
            GenerateSkipNullProperties: GetValue(options.GlobalOptions, GenerateSkipNullPropertiesKey, false),
            GenerateTransitive: GetValue(options.GlobalOptions, GenerateTransitiveKey, false),
            OverrideBeginScopeBehavior: GetValue(options.GlobalOptions, OverrideBeginScopeBehavior, true)
        ));

    private static bool GetValue(AnalyzerConfigOptions options, string key, bool defaultValue = true) =>
        options.TryGetValue(key, out var value) ? IsFeatureEnabled(value, defaultValue) : defaultValue;

    private static bool IsFeatureEnabled(string value, bool defaultValue) =>
        StringComparer.OrdinalIgnoreCase.Equals("enable", value)
        || StringComparer.OrdinalIgnoreCase.Equals("enabled", value)
        || StringComparer.OrdinalIgnoreCase.Equals("true", value)
        || (bool.TryParse(value, out var boolVal) ? boolVal : defaultValue);
}
