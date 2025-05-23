using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.Models;

internal readonly record struct CallLocation(
    string FilePath,
    int Line,
    int Character,
    string InterceptableLocationSyntax,
    Location Context
)
{
    public bool Equals(CallLocation other) =>
        FilePath == other.FilePath &&
        Line == other.Line &&
        Character == other.Character &&
        InterceptableLocationSyntax == other.InterceptableLocationSyntax;

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = FilePath.GetHashCode();
            hashCode = (hashCode * 397) ^ Line;
            hashCode = (hashCode * 397) ^ Character;
            hashCode = (hashCode * 397) ^ InterceptableLocationSyntax.GetHashCode();
            return hashCode;
        }
    }
}
