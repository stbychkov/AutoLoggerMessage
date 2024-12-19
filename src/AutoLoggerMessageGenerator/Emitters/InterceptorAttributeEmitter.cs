using System.CodeDom.Compiler;
using System.IO;

namespace AutoLoggerMessageGenerator.Emitters;

internal static class InterceptorAttributeEmitter
{
    public static string Emit()
    {
        using var sb = new IndentedTextWriter(new StringWriter());

        sb.WriteLine(Constants.GeneratedFileHeader);

        sb.WriteLine($"namespace {Constants.InterceptorNamespace}");
        sb.WriteLine('{');
        sb.Indent++;

        sb.WriteLine(Constants.GeneratedCodeAttribute);
        sb.WriteLine(Constants.EditorNotBrowsableAttribute);
        sb.WriteLine("[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]");
        sb.WriteLine($"internal sealed class {Constants.InterceptorAttributeName} : Attribute");
        sb.WriteLine('{');
        sb.Indent++;

        sb.WriteLine($"public {Constants.InterceptorAttributeName}(int version, string data) {{}}");

        sb.Indent--;
        sb.WriteLine('}');

        sb.Indent--;
        sb.WriteLine('}');

        return sb.InnerWriter.ToString()!;
    }
}
