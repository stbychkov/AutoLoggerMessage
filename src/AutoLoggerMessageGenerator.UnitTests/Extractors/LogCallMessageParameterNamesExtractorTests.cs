using AutoLoggerMessageGenerator.Extractors;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

internal class LogCallMessageParameterNamesExtractorTests
{
    [Test]
    [Arguments("Hello world {1}, {2}!", new[] { "1", "2" })]
    [Arguments("{param1} and {param2} are here", new[] { "param1", "param2" })]
    [Arguments("No parameters in this string", new string[0])]
    [Arguments("Edge case with empty braces {}", new[] { "" })]
    [Arguments("{0}{1}{2}{3}", new[] { "0", "1", "2", "3" })]
    [Arguments("{a} mixed with text {b}", new[] { "a", "b" })]
    [Arguments("double {{escaped braces}}", new[] {"{escaped braces"})]
    [Arguments("{name1}-{name2}-{name3}", new[] { "name1", "name2", "name3" })]
    [Arguments("Duplicate {param} and {param} again", new[] { "param", "param" })]
    [Arguments("", new string[0])]
    [Arguments(null, new string[0])]
    public async Task Extract_WithGivenMessage_ShouldReturnGivenMessageParameterNames(string? message, params string[] expectedMessageParameters)
    {
        var result = LogCallMessageParameterNamesExtractor.Extract(message);
        await Assert.That(result).IsEquivalentTo(expectedMessageParameters);
    }
}
