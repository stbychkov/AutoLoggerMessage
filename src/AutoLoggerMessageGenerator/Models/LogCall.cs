using System;
using System.Collections.Immutable;
using System.Linq;

namespace AutoLoggerMessageGenerator.Models;

internal readonly record struct LogCall(
    // Need this property for backtracking diagnostic reports
    Guid Id,
    LogCallLocation Location,
    string Namespace,
    string ClassName,
    string Name,
    string LogLevel,
    string Message,
    ImmutableArray<LogCallParameter> Parameters
)
{
    public bool Equals(LogCall other) =>
        Location.Equals(other.Location) &&
        Namespace == other.Namespace &&
        ClassName == other.ClassName &&
        Name == other.Name &&
        LogLevel == other.LogLevel &&
        Message == other.Message &&
        Parameters.SequenceEqual(other.Parameters);

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Location.GetHashCode();
            hashCode = (hashCode * 397) ^ Namespace.GetHashCode();
            hashCode = (hashCode * 397) ^ ClassName.GetHashCode();
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            hashCode = (hashCode * 397) ^ LogLevel.GetHashCode();
            hashCode = (hashCode * 397) ^ Message.GetHashCode();
            hashCode = (hashCode * 397) ^ Parameters.GetHashCode();
            return hashCode;
        }
    }
};

