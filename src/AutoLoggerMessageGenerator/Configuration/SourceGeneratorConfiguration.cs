namespace AutoLoggerMessageGenerator.Configuration;

internal record struct SourceGeneratorConfiguration
(
    // You can disable generating of the interceptor attribute by setting this to false
    // You might need it if you already have this attribute in your project
    bool GenerateInterceptorAttribute,
    bool GenerateSkipEnabledCheck,
    bool GenerateOmitReferenceName,
    bool GenerateSkipNullProperties,
    bool GenerateTransitive
);
