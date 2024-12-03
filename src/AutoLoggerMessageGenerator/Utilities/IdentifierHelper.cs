using System;
using System.Text.RegularExpressions;

namespace AutoLoggerMessageGenerator.Utilities;

internal static partial class IdentifierHelper
{
    public static string ToValidCSharpMethodName(string? input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentNullException(nameof(input), @"Method name cannot be empty");

        var sanitizedInput = Regex.Replace(input, @"[^a-zA-Z0-9_]", "_");
        if (!char.IsLetter(sanitizedInput[0]) && sanitizedInput[0] != '_')
            sanitizedInput = "_" + sanitizedInput;

        return sanitizedInput;
    }

    public static bool IsValidCSharpParameterName(string name) =>
        !string.IsNullOrEmpty(name) && IsValidCSharpParameterNameRegex().IsMatch(name);

    [GeneratedRegex(@"^@?[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex IsValidCSharpParameterNameRegex();
}
