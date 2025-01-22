using System.Text.RegularExpressions;

namespace AutoLoggerMessageGenerator.UnitTests.Scrubbers;

public static partial class GeneratedCodeAttributeScrubber
{
    [GeneratedRegex($""""{nameof(AutoLoggerMessageGenerator)}", "\d+.\d+.\d+.\d?"""")]
    private static partial Regex GeneratedCodeAttributeVersionRegex();

    public static SettingsTask AddCodeGeneratedAttributeScrubber(this SettingsTask task)
    {
        task.ScrubLinesWithReplace(line => GeneratedCodeAttributeVersionRegex().Replace(
            line,
            $""""{nameof(AutoLoggerMessageGenerator)}", "1.2.3.4""""));
        return task;
    }
}
