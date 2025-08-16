using System.CodeDom.Compiler;

namespace AutoLoggerMessageGenerator.Emitters;

internal static class InterceptorAttributeEmitter
{
    public static string Emit()
    {
        using var sb = new IndentedTextWriter(new StringWriter());

        sb.WriteLine(Constants.GeneratedFileHeader);
        sb.WriteLine();

        sb.WriteLine($"namespace {Constants.InterceptorNamespace}");
        sb.WriteLine('{');
        sb.Indent++;

        #if EMBEDDED
        sb.WriteLine(Constants.EmbeddedAttribute);
        #endif

        sb.WriteLine(Constants.GeneratedCodeAttribute);
        sb.WriteLine(Constants.EditorNotBrowsableAttribute);
        sb.WriteLine("[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]");
        sb.WriteLine($"internal sealed class {Constants.InterceptorAttributeName} : Attribute");
        sb.WriteLine('{');
        sb.Indent++;

        #if PATH_BASED_INTERCEPTORS
            sb.WriteLine($"public {Constants.InterceptorAttributeName}(string filePath, int line, int character) {{}}");
        #elif HASH_BASED_INTERCEPTORS
            sb.WriteLine($"public {Constants.InterceptorAttributeName}(int version, string data) {{}}");
        #endif

        sb.Indent--;
        sb.WriteLine('}');

        sb.Indent--;
        sb.WriteLine('}');

        return sb.InnerWriter.ToString()!;
    }
}
