using System.CodeDom.Compiler;
using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Models;
using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.Emitters;

internal static class LoggerScopesEmitter
{
    public static string Emit(ImmutableArray<LoggerScopeCall> loggerScopes)
    {
        using var sb = new IndentedTextWriter(new StringWriter());

        sb.WriteLine(GeneratedFileHeader);
        sb.WriteLine();

        sb.WriteLine($"namespace {GeneratorNamespace}");
        sb.WriteLine('{');
        sb.Indent++;

        #if EMBEDDED
        sb.WriteLine(EmbeddedAttribute);
        #endif

        sb.WriteLine(Constants.GeneratedCodeAttribute);
        sb.WriteLine(EditorNotBrowsableAttribute);
        sb.WriteLine($"internal static class {LoggerScopesGeneratorName}");
        sb.WriteLine('{');
        sb.Indent++;

        foreach (var loggerScope in loggerScopes)
        {
            var parameters = loggerScope.Parameters.Where(c => c.Name != MessageParameterName).ToArray();

            var genericTypes = string.Join(", ", parameters.Select(c => c.NativeType));
            var defineScopeGenericTypes = genericTypes == string.Empty ? string.Empty : $"<{genericTypes}>";
            genericTypes = string.IsNullOrEmpty(genericTypes) ? string.Empty : $", {genericTypes}";

            var parameterList = string.Join(", ", parameters.Select(c => $"{c.NativeType} {c.Name}"));
            parameterList = string.IsNullOrEmpty(parameterList) ? string.Empty : $", {parameterList}";

            var parameterValues = string.Join(", ", parameters.Where(c => c.Name != MessageParameterName).Select(c => c.Name));
            parameterValues = string.IsNullOrEmpty(parameterValues) ? string.Empty : $", {parameterValues}";

            var loggerDefineFunctorName = $"_{loggerScope.GeneratedMethodName}";
            sb.WriteLine($"private static readonly Func<ILogger{genericTypes}, IDisposable?> {loggerDefineFunctorName} = LoggerMessage.DefineScope{defineScopeGenericTypes}(\"{loggerScope.Message}\");");

            sb.WriteLine($"public static IDisposable? {loggerScope.GeneratedMethodName}(ILogger @logger{parameterList})");
            sb.WriteLine('{');
            sb.Indent++;

            sb.WriteLine($"return {loggerDefineFunctorName}(@logger{parameterValues});");

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
