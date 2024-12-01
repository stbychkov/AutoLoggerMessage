using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.Utilities;

namespace AutoLoggerMessageGenerator.Emitters;

internal class LoggerInterceptorsEmitter
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
            sb.WriteLine($"[{Constants.InterceptorNamespace}.{Constants.InterceptorAttributeName}(");
            sb.Indent++;
            sb.WriteLine($"filePath: \"{logCall.Location.FilePath}\",");
            sb.WriteLine($"line: {logCall.Location.Line}, character: {logCall.Location.Character})]");
            sb.Indent--;

            var parameters = string.Join(", ", logCall.Parameters.Select((c, i) => $"{c.Type} {c.Name}"));
            parameters = string.IsNullOrEmpty(parameters) ? string.Empty : $", {parameters}";
            
            var parameterValues = string.Join(", ", logCall.Parameters.Select((c, i) => c.Name));
            parameterValues = string.IsNullOrEmpty(parameterValues) ? string.Empty : $", {parameterValues}";
            
            var methodName = IdentifierHelper.ToValidCSharpMethodName(
                $"{Constants.LogMethodPrefix}{logCall.Namespace}{logCall.ClassName}_{logCall.Location.Line}_{logCall.Location.Character}"
            );

            sb.WriteLine($"public static void {methodName}(this ILogger @logger{parameters})");
            sb.WriteLine('{');
            sb.Indent++;

            sb.WriteLine($"{Constants.GeneratorNamespace}.{Constants.LoggerClassName}.{methodName}(@logger{parameterValues});");

            sb.Indent--;
            sb.WriteLine('}');
            sb.WriteLine();
        }

        sb.Indent--;
        sb.WriteLine('}');

        sb.Indent--;
        sb.WriteLine('}');

        return sb.InnerWriter.ToString();
    }
}
