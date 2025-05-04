using System.CodeDom.Compiler;
using AutoLoggerMessageGenerator.Models;

namespace AutoLoggerMessageGenerator.Emitters;

internal static class LoggerScopeInterceptorsEmitter
{
    public static string Emit(IEnumerable<LoggerScopeCall> loggerScopes)
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
        sb.WriteLine("internal static class LoggerScopeInterceptors");
        sb.WriteLine('{');
        sb.Indent++;

        foreach (var loggerScope in loggerScopes)
        {
            sb.WriteLine(loggerScope.Location.InterceptableLocationSyntax);

            var parameters = string.Join(", ", loggerScope.Parameters.Select((c, i) => $"{c.NativeType} {c.Name}"));
            parameters = string.IsNullOrEmpty(parameters) ? string.Empty : $", {parameters}";

            var parameterValues = string.Join(", ", loggerScope.Parameters
                .Where(c => c.Name != Constants.MessageParameterName)
                .Select((c, i) => c.Name));
            parameterValues = string.IsNullOrEmpty(parameterValues) ? string.Empty : $", {parameterValues}";

            sb.WriteLine($"public static IDisposable? {loggerScope.GeneratedMethodName}(this ILogger {Constants.LoggerParameterName}{parameters})");
            sb.WriteLine('{');
            sb.Indent++;

            sb.WriteLine($"return {Constants.GeneratorNamespace}.{Constants.LoggerScopesGeneratorName}.{loggerScope.GeneratedMethodName}({Constants.LoggerParameterName}{parameterValues});");

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
