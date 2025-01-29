using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.Models;

internal readonly record struct LogCallLocation(
    string FilePath,
    int Line,
    int Character,
    string InterceptableLocationSyntax,
    Location Context
)
{
    public readonly bool Equals(LogCallLocation other) =>
        FilePath == other.FilePath &&
        Line == other.Line &&
        Character == other.Character &&
        InterceptableLocationSyntax == other.InterceptableLocationSyntax;

    public override int GetHashCode()
    {
        return HashCode.Combine(FilePath, Line, Character, InterceptableLocationSyntax);
    }
}
