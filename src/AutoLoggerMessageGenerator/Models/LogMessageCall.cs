using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Utilities;

namespace AutoLoggerMessageGenerator.Models;

internal readonly record struct LogMessageCall(
    // Need this property for backtracking diagnostic reports
    Guid Id,
    CallLocation Location,
    string Namespace,
    string ClassName,
    string MethodName,
    string LogLevel,
    string Message,
    ImmutableArray<CallParameter> Parameters
)
{
    public string GeneratedMethodName =>
        IdentifierHelper.ToValidCSharpMethodName(
            $"{Constants.LogMethodPrefix}{Namespace}{ClassName}_{Location.Line}_{Location.Character}"
        );

    public bool Equals(LogMessageCall other) =>
        Location.Equals(other.Location) &&
        Namespace == other.Namespace &&
        ClassName == other.ClassName &&
        MethodName == other.MethodName &&
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
            hashCode = (hashCode * 397) ^ MethodName.GetHashCode();
            hashCode = (hashCode * 397) ^ LogLevel.GetHashCode();
            hashCode = (hashCode * 397) ^ Message.GetHashCode();
            hashCode = (hashCode * 397) ^ Parameters.GetHashCode();
            return hashCode;
        }
    }
};

