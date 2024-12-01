using System.Collections.Frozen;
using System.Collections.Generic;

namespace AutoLoggerMessageGenerator.Benchmarks;

internal record ProjectConfiguration
{
    public required string Name { get; init; }
    
    public IReadOnlySet<string> References { get; init; } = FrozenSet<string>.Empty;
}
