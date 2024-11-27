using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoLoggerMessageGenerator.Configuration;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.ReferenceAnalyzer;
using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.Caching;

internal class InputSourceComparer : IEqualityComparer<(Compilation Compilation, (SourceGeneratorConfiguration Configuration, (ImmutableArray<Reference> References, ImmutableArray<LogCall> LogCalls) Others) Others)>
{
    public bool Equals((Compilation Compilation, (SourceGeneratorConfiguration Configuration, (ImmutableArray<Reference> References, ImmutableArray<LogCall> LogCalls) Others) Others) x, (Compilation Compilation, (SourceGeneratorConfiguration Configuration, (ImmutableArray<Reference> References, ImmutableArray<LogCall> LogCalls) Others) Others) y) =>
        x.Others.Configuration == y.Others.Configuration &&
        x.Others.Others.References.SequenceEqual(y.Others.Others.References) &&
        x.Others.Others.LogCalls.SequenceEqual(y.Others.Others.LogCalls);

    public int GetHashCode((Compilation Compilation, (SourceGeneratorConfiguration Configuration, (ImmutableArray<Reference> References, ImmutableArray<LogCall> LogCalls) Others) Others) obj) =>
        obj.Item2.GetHashCode();
}
