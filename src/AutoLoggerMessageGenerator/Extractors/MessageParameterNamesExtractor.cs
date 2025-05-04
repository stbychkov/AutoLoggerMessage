using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace AutoLoggerMessageGenerator.Extractors;

internal static class MessageParameterNamesExtractor
{
    public static ImmutableArray<string> Extract(string? message) =>
        MessageArgumentRegex.Matches(message ?? string.Empty)
            .OfType<Match>()
            .Select(c => c.Groups[1].Value)
            .ToImmutableArray();

    private static Regex MessageArgumentRegex => new(@"\{(.*?)\}", RegexOptions.Compiled);
}

