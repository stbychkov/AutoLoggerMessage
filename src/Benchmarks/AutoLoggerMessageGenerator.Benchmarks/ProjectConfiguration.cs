using System.Collections.Frozen;

namespace AutoLoggerMessageGenerator.Benchmarks;

internal record ProjectConfiguration
{
    public required string Name { get; init; }
    
    public IReadOnlySet<string> References { get; init; } = FrozenSet<string>.Empty;
}
