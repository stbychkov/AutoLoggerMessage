using System.CodeDom.Compiler;
using Microsoft.Extensions.Logging;
using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.Emitters;

/// Generates the <see cref="GenericLoggerScopeExtensions"/> class.
/// Provides type-specific logging method overrides to avoid boxing.
/// The <see cref="GenericLoggerScopeExtensions"/> class is pre-generated and saved in the solution
/// to prevent excessive code duplication when multiple projects use the same library
/// To run this emitter you need to run LoggerScopeExtensionEmitterTests and take it from the snapshot
internal static class GenericLoggerScopeExtensionsEmitter
{
    public const string ClassName = "GenericLoggerScopeExtensions";

    public static string Emit()
    {
        using var sb = new IndentedTextWriter(new StringWriter());

        sb.WriteLine(GeneratedFileHeader);
        sb.WriteLine(JetBrainsAnnotationsImport);
        sb.WriteLine();

        sb.WriteLine($"namespace {DefaultLoggingNamespace}");
        sb.WriteLine('{');
        sb.Indent++;

        sb.WriteLine(Constants.GeneratedCodeAttribute);
        sb.WriteLine(EditorNotBrowsableAttribute);
        sb.WriteLine(DebuggerStepThroughAttribute);
        sb.WriteLine(ExcludeFromCoverageAttribute);
        sb.WriteLine($"public static class {ClassName}");
        sb.WriteLine('{');
        sb.Indent++;

        for (int i = 1; i <= MaxLogParameters; i++)
        {
            var parameters = Enumerable.Range(0, i).ToArray();

            var genericTypesDefinition = string.Join(", ", parameters.Select(ix => $"T{ix}"));
            genericTypesDefinition = string.IsNullOrEmpty(genericTypesDefinition)
                ? string.Empty
                : $"<{genericTypesDefinition}>";

            var genericParametersDefinition =
                string.Join(", ", parameters.Select(ix => $"T{ix} {ParameterName}{ix}"));
            genericParametersDefinition = string.IsNullOrEmpty(genericParametersDefinition)
                ? string.Empty
                : $", {genericParametersDefinition}";

            var objectParameters = string.Join(", ", parameters.Select(ix => $"{ParameterName}{ix}"));
            objectParameters = string.IsNullOrEmpty(objectParameters)
                ? string.Empty
                : $", new object?[] {{ {objectParameters} }}";

            sb.WriteLine($"public static IDisposable? BeginScope{genericTypesDefinition}(this ILogger {LoggerParameterName}, {MessageTemplateDecorator} string {MessageParameterName}{genericParametersDefinition})");
            sb.WriteLine('{');
            sb.Indent++;

            sb.WriteLine($"return {DefaultLoggingNamespace}.{nameof(LoggerExtensions)}.{nameof(LoggerExtensions.BeginScope)}({LoggerParameterName}, {MessageParameterName}{objectParameters});");

            sb.Indent--;
            sb.WriteLine('}');

            sb.WriteLine();
        }

        sb.Indent--;
        sb.WriteLine('}');

        sb.Indent--;
        sb.WriteLine('}');

        return sb.InnerWriter.ToString()!;
    }
}
