using System.CodeDom.Compiler;
using System.IO;
using System.Linq;

namespace AutoLoggerMessageGenerator.Emitters;

#if DEBUG

/// Generates the <see cref="LoggerExtensions"/> class.
/// Provides type-specific logging method overrides to avoid boxing.
/// The <see cref="LoggerExtensions"/> class is pre-generated and saved in the solution
/// to prevent excessive code duplication when multiple projects use the same library
internal class LoggerExtensionsEmitter
{
    public const string ArgumentName = "arg";

    public static string Emit()
    {
        using var sb = new IndentedTextWriter(new StringWriter());

        sb.WriteLine(Constants.GeneratedFileHeader);

        sb.WriteLine("namespace Microsoft.Extensions.Logging");
        sb.WriteLine('{');
        sb.Indent++;

        sb.WriteLine(Constants.GeneratedCodeAttribute);
        sb.WriteLine(Constants.EditorNotBrowsableAttribute);
        sb.WriteLine(Constants.DebuggerStepThroughAttribute);
        sb.WriteLine(Constants.ExcludeFromCoverageAttribute);
        sb.WriteLine("internal static class LoggerExtensions");
        sb.WriteLine('{');
        sb.Indent++;

        string[] logLevels = ["Trace", "Debug", "Information", "Warning", "Error", "Critical"];

        (string Type, string Name)[][] fixedParametersOverloads =
        [
            [("string?", "@message")],
            [("Exception?", "@exception"), ("string?", "@message")],
            [("EventId", "@eventId"), ("string?", "@message")],
            [("EventId", "@eventId"), ("Exception?", "@exception"), ("string?", "@message")]
        ];

        foreach (var fixedParametersOverload in fixedParametersOverloads)
        {
            var fixedParametersDefinition = string.Join(", ", fixedParametersOverload.Select(o => $"{o.Type} {o.Name}"));
            var fixedParameters = string.Join(", ", fixedParametersOverload.Select(o => o.Name));

            foreach (var logLevel in logLevels)
            {
                for (int i = 0; i < Constants.MaxLogParameters; i++)
                {
                    var parameters = Enumerable.Range(1, i + 1);
                    var genericDefinition = string.Join(", ", parameters.Select(ix => $"T{ix}"));
                    var genericParametersDefinition = string.Join(", ", parameters.Select(ix => $"T{ix} @{ArgumentName}{ix}"));

                    sb.WriteLine($"public static void Log{logLevel}<{genericDefinition}>(this ILogger @logger, {fixedParametersDefinition}, {genericParametersDefinition})");
                    sb.WriteLine('{');
                    sb.Indent++;

                    sb.WriteLine($"@logger.Log{logLevel}({fixedParameters}, new object?[] {{ {string.Join(", ", parameters.Select(ix => $"@{ArgumentName}{ix}"))} }});");

                    sb.Indent--;
                    sb.WriteLine('}');

                    sb.WriteLine();
                }

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
}

#endif
