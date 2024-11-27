using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.Models;

internal readonly record struct LogCallLocation(
    string FilePath,
    int Line,
    int Character,
    Location Context
)
{
    public readonly bool Equals(LogCallLocation other) =>
        FilePath == other.FilePath &&
        Line == other.Line &&
        Character == other.Character;

    public readonly override int GetHashCode()
    {
        unchecked
        {
            var hashCode = FilePath.GetHashCode();
            hashCode = (hashCode * 397) ^ Line;
            hashCode = (hashCode * 397) ^ Character;
            return hashCode;
        }
    }
}
