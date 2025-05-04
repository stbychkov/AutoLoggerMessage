using System.CodeDom.Compiler;
using AutoLoggerMessageGenerator.Models;

namespace AutoLoggerMessageGenerator.Emitters;

internal static class LoggerInterceptorsEmitter
{
    public static string Emit(IEnumerable<LogMessageCall> logCalls)
    {
        using var sb = new IndentedTextWriter(new StringWriter());

        sb.WriteLine(Constants.GeneratedFileHeader);

        sb.WriteLine($"namespace {Constants.GeneratorNamespace}");
        sb.WriteLine('{');
        sb.Indent++;

        sb.WriteLine(Constants.GeneratedCodeAttribute);
        sb.WriteLine(Constants.EditorNotBrowsableAttribute);
        sb.WriteLine(Constants.ExcludeFromCoverageAttribute);
        sb.WriteLine(Constants.DebuggerStepThroughAttribute);
        sb.WriteLine("internal static class LoggerInterceptors");
        sb.WriteLine('{');
        sb.Indent++;

        foreach (var logCall in logCalls)
        {
            sb.WriteLine(Constants.EditorNotBrowsableAttribute);
            sb.WriteLine(logCall.Location.InterceptableLocationSyntax);

            var parameters = string.Join(", ", logCall.Parameters.Select((c, i) => $"{c.NativeType} {c.Name}"));
            parameters = string.IsNullOrEmpty(parameters) ? string.Empty : $", {parameters}";

            var parameterValues = string.Join(", ", logCall.Parameters
                .Where(c => !Constants.LoggerMessageAttributeParameterTypes.Contains(c.Type))
                .Select((c, i) => c.Name));
            parameterValues = string.IsNullOrEmpty(parameterValues) ? string.Empty : $", {parameterValues}";

            sb.WriteLine($"public static void {logCall.GeneratedMethodName}(this ILogger {Constants.LoggerParameterName}{parameters})");
            sb.WriteLine('{');
            sb.Indent++;

            sb.WriteLine($"{Constants.GeneratorNamespace}.{Constants.LoggerClassName}.{logCall.GeneratedMethodName}({Constants.LoggerParameterName}{parameterValues});");

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
