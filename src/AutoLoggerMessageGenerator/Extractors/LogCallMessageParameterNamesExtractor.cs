using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace AutoLoggerMessageGenerator.Extractors;

internal static partial class LogCallMessageParameterNamesExtractor
{
    public static ImmutableArray<string> Extract(string? message) =>
        [..MessageArgumentRegex().Matches(message ?? string.Empty).Select(c => c.Groups[1].Value)];

    [GeneratedRegex(@"\{(.*?)\}", RegexOptions.Compiled)]
    private static partial Regex MessageArgumentRegex();
}

