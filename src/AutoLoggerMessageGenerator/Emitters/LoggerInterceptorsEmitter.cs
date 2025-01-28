using System.CodeDom.Compiler;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.Utilities;

namespace AutoLoggerMessageGenerator.Emitters;

internal static class LoggerInterceptorsEmitter
{
    public static string Emit(IEnumerable<LogCall> logCalls)
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

            var methodName = IdentifierHelper.ToValidCSharpMethodName(
                $"{Constants.LogMethodPrefix}{logCall.Namespace}{logCall.ClassName}_{logCall.Location.Line}_{logCall.Location.Character}"
            );

            sb.WriteLine($"public static void {methodName}(this ILogger {Constants.LoggerParameterName}{parameters})");
            sb.WriteLine('{');
            sb.Indent++;

            sb.WriteLine($"{Constants.GeneratorNamespace}.{Constants.LoggerClassName}.{methodName}({Constants.LoggerParameterName}{parameterValues});");

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
