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

    public override int GetHashCode() =>
        (Location, Namespace, ClassName, MethodName, LogLevel, Message, Parameters).GetHashCode();
};

