using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutoLoggerMessageGenerator.Extractors;

internal static class LogCallMessageParameterNamesExtractor
{
    public static ImmutableArray<string> Extract(string? message) =>
        [..MessageArgumentRegex.Matches(message ?? string.Empty)
            .OfType<Match>()
            .Select(c => c.Groups[1].Value)];

    private static Regex MessageArgumentRegex => new(@"\{(.*?)\}", RegexOptions.Compiled);
}

