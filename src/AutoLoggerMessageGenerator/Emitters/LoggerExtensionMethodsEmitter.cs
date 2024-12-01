using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.Emitters;

/// Generates the <see cref="LoggerExtensions"/> class.
/// Provides type-specific logging method overrides to avoid boxing.
/// The <see cref="LoggerExtensions"/> class is pre-generated and saved in the solution
/// to prevent excessive code duplication when multiple projects use the same library
/// To run this emitter you need to run LoggerExtensionEmitterTests and take it from the snapshot/>
internal class LoggerExtensionsEmitter
{
    public const string ClassName = "GenericLoggerExtensions";
    public const string ArgumentName = "arg";

    public static string Emit()
    {
        using var sb = new IndentedTextWriter(new StringWriter());

        sb.WriteLine(Constants.GeneratedFileHeader);

        sb.WriteLine($"namespace {Constants.DefaultLoggingNamespace}");
        sb.WriteLine('{');
        sb.Indent++;

        sb.WriteLine(Constants.GeneratedCodeAttribute);
        sb.WriteLine(Constants.EditorNotBrowsableAttribute);
        sb.WriteLine(Constants.DebuggerStepThroughAttribute);
        sb.WriteLine(Constants.ExcludeFromCoverageAttribute);
        sb.WriteLine($"public static class {ClassName}");
        sb.WriteLine('{');
        sb.Indent++;

        string[] logLevels = ["Trace", "Debug", "Information", "Warning", "Error", "Critical"];

        (string Type, string Name)[][] fixedParametersOverloads =
        [
            [("string", "@message")],
            [("Exception?", "@exception"), ("string", "@message")],
            [("EventId", "@eventId"), ("string", "@message")],
            [("EventId", "@eventId"), ("Exception?", "@exception"), ("string", "@message")]
        ];

        foreach (var fixedParametersOverload in fixedParametersOverloads)
        {
            var fixedParametersDefinition =
                string.Join(", ", fixedParametersOverload.Select(o => $"{o.Type} {o.Name}"));
            var fixedParameters = string.Join(", ", fixedParametersOverload.Select(o => o.Name));

            for (int i = 0; i <= Constants.MaxLogParameters; i++)
            {
                var parameters = Enumerable.Range(0, i).ToArray();

                var genericTypesDefinition = string.Join(", ", parameters.Select(ix => $"T{ix}"));
                genericTypesDefinition = string.IsNullOrEmpty(genericTypesDefinition)
                    ? string.Empty
                    : $"<{genericTypesDefinition}>";

                var genericParametersDefinition =
                    string.Join(", ", parameters.Select(ix => $"T{ix} @{ArgumentName}{ix}"));
                genericParametersDefinition = string.IsNullOrEmpty(genericParametersDefinition)
                    ? string.Empty
                    : $", {genericParametersDefinition}";

                var objectParameters = string.Join(", ", parameters.Select(ix => $"@{ArgumentName}{ix}"));
                objectParameters = string.IsNullOrEmpty(objectParameters)
                    ? string.Empty
                    : $", new object?[] {{ {objectParameters} }}";
                
                foreach (var logLevel in logLevels)
                {
                    GenerateLogMethodWithLogLevelInName(sb, logLevel, genericTypesDefinition,
                        fixedParametersDefinition, genericParametersDefinition, fixedParameters, objectParameters);

                    sb.WriteLine();
                }
                
                GenerateLogMethodWithLogLevelParameter(sb, genericTypesDefinition, fixedParametersDefinition, 
                    genericParametersDefinition, fixedParameters, objectParameters);

                sb.WriteLine();
            }

            sb.WriteLine();
        }

        sb.Indent--;
        sb.WriteLine('}');

        sb.Indent--;
        sb.WriteLine('}');

        return sb.InnerWriter.ToString();
    }

    private static void GenerateLogMethodWithLogLevelInName(IndentedTextWriter sb, string logLevel,
        string genericTypesDefinition, string fixedParametersDefinition, string genericParametersDefinition,
        string fixedParameters, string objectParameters)
    {
        sb.WriteLine(
            $"public static void Log{logLevel}{genericTypesDefinition}(this ILogger @logger, {fixedParametersDefinition}{genericParametersDefinition})");
        sb.WriteLine('{');
        sb.Indent++;

        sb.WriteLine($"{Constants.DefaultLoggingNamespace}.{nameof(LoggerExtensions)}.Log{logLevel}(@logger, {fixedParameters}{objectParameters});");

        sb.Indent--;
        sb.WriteLine('}');
    }

    private static void GenerateLogMethodWithLogLevelParameter(IndentedTextWriter sb, string genericTypesDefinition,
        string fixedParametersDefinition, string genericParametersDefinition, string fixedParameters,
        string objectParameters)
    {
        sb.WriteLine(
            $"public static void Log{genericTypesDefinition}(this ILogger @logger, {Constants.DefaultLoggingNamespace}.LogLevel @logLevel, {fixedParametersDefinition}{genericParametersDefinition})");
        sb.WriteLine('{');
        sb.Indent++;

        sb.WriteLine($"{Constants.DefaultLoggingNamespace}.{nameof(LoggerExtensions)}.{nameof(LoggerExtensions.Log)}(@logger, @logLevel, {fixedParameters}{objectParameters});");

        sb.Indent--;
        sb.WriteLine('}');
    }
}
