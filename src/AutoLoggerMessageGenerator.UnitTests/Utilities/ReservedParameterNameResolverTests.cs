using AutoLoggerMessageGenerator.Utilities;
using FluentAssertions;

namespace AutoLoggerMessageGenerator.UnitTests.Utilities;

public class ReservedParameterNameResolverTests
{
    [Theory]
    [InlineData(0, new[] {"a","b","c"})]
    [InlineData(1, new[] {"@message","b","c"})]
    [InlineData(2, new[] {"@message_","@message","c"})]
    [InlineData(4, new[] {"@message", "@message___", "@message__"})]
    [InlineData(4, new[] {"@message", "@exception___", "@eventId_"})]
    public void WithGivenTemplateParameterNames_ShouldCalculateExpectedPrefixLength(int expectedPrefixLength, params string[] templateParameterNames)
    {
        var actualPrefixLength = ReservedParameterNameResolver.GenerateUniqueIdentifierSuffix(templateParameterNames);
        actualPrefixLength.Should().HaveLength(expectedPrefixLength);
    }
}
