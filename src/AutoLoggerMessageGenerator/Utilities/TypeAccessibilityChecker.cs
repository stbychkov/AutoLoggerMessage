using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.Utilities;

internal static class TypeAccessibilityChecker
{
    public static bool IsAccessible(ITypeSymbol typeSymbol) =>
        typeSymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal &&
        (typeSymbol.ContainingType is null || IsAccessible(typeSymbol.ContainingType));
}
