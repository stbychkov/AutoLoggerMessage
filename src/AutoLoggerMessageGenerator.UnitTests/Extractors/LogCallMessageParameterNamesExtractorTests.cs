using AutoLoggerMessageGenerator.Extractors;
using FluentAssertions;

namespace AutoLoggerMessageGenerator.UnitTests.Extractors;

public class LogCallMessageParameterNamesExtractorTests
{
    [Theory]
    [InlineData("Hello world {1}, {2}!", new[] { "1", "2" })]
    [InlineData("{param1} and {param2} are here", new[] { "param1", "param2" })]
    [InlineData("No parameters in this string", new string[0])]
    [InlineData("Edge case with empty braces {}", new[] { "" })]
    [InlineData("{0}{1}{2}{3}", new[] { "0", "1", "2", "3" })]
    [InlineData("{a} mixed with text {b}", new[] { "a", "b" })]
    [InlineData("double {{escaped braces}}", new[] {"{escaped braces"})]
    [InlineData("{name1}-{name2}-{name3}", new[] { "name1", "name2", "name3" })]
    [InlineData("Duplicate {param} and {param} again", new[] { "param", "param" })]
    [InlineData("", new string[0])]
    [InlineData(null, new string[0])]
    public void Extract_WithGivenMessage_ShouldReturnGivenMessageParameterNames(string message, params string[] expectedMessageParameters)
    {
        var result = LogCallMessageParameterNamesExtractor.Extract(message);
        result.Should().BeEquivalentTo(expectedMessageParameters);
    }
}
