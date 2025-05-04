using InputSource = (
    Microsoft.CodeAnalysis.Compilation Compilation,
    (
        AutoLoggerMessageGenerator.Configuration.SourceGeneratorConfiguration Configuration,
        System.Collections.Immutable.ImmutableArray<AutoLoggerMessageGenerator.Models.LoggerScopeCall> LoggerScopes
    ) Others
);

namespace AutoLoggerMessageGenerator.Caching;

internal class LogScopeDefinitionInputSourceComparer : IEqualityComparer<InputSource>
{
    public bool Equals(InputSource x, InputSource y) =>
        x.Others.Configuration == y.Others.Configuration &&
        x.Others.LoggerScopes.SequenceEqual(y.Others.LoggerScopes);

    public int GetHashCode(InputSource obj) =>
        obj.Item2.GetHashCode();
}
