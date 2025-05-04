using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Utilities;

namespace AutoLoggerMessageGenerator.Models;

internal readonly record struct LoggerScopeCall(
    CallLocation Location,
    string Namespace,
    string ClassName,
    string MethodName,
    string Message,
    ImmutableArray<CallParameter> Parameters
)
{
    public string GeneratedMethodName =>
        IdentifierHelper.ToValidCSharpMethodName(
            $"{Constants.LogScopeMethodPrefix}{Namespace}{ClassName}_{Location.Line}_{Location.Character}"
        );

    public bool Equals(LoggerScopeCall other) =>
        Location.Equals(other.Location) &&
        Namespace == other.Namespace &&
        ClassName == other.ClassName &&
        MethodName == other.MethodName &&
        Message == other.Message &&
        Parameters.SequenceEqual(other.Parameters);

    public override int GetHashCode() => (Location, Namespace, ClassName, MethodName, Message, Parameters).GetHashCode();
};

