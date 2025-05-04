using InputSource = (
    Microsoft.CodeAnalysis.Compilation Compilation,
    (
        AutoLoggerMessageGenerator.Configuration.SourceGeneratorConfiguration Configuration,
        (
            System.Collections.Immutable.ImmutableArray<AutoLoggerMessageGenerator.ReferenceAnalyzer.Reference> References,
            System.Collections.Immutable.ImmutableArray<AutoLoggerMessageGenerator.Models.LogMessageCall> LogCalls
        ) Others
    ) Others
);

namespace AutoLoggerMessageGenerator.Caching;

internal class LogCallInputSourceComparer : IEqualityComparer<InputSource>
{
    public bool Equals(InputSource x, InputSource y) =>
        x.Others.Configuration == y.Others.Configuration &&
        x.Others.Others.References.SequenceEqual(y.Others.Others.References) &&
        x.Others.Others.LogCalls.SequenceEqual(y.Others.Others.LogCalls);

    public int GetHashCode(InputSource obj) =>
        obj.Item2.GetHashCode();
}
