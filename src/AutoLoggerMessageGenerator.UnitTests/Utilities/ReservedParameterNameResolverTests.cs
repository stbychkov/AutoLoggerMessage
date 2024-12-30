using AutoLoggerMessageGenerator.Utilities;

namespace AutoLoggerMessageGenerator.UnitTests.Utilities;

internal class ReservedParameterNameResolverTests
{
    [Test]
    [Arguments(0, new[] {"a","b","c"})]
    [Arguments(1, new[] {"@message","b","c"})]
    [Arguments(2, new[] {"@message_","@message","c"})]
    [Arguments(4, new[] {"@message", "@message___", "@message__"})]
    [Arguments(4, new[] {"@message", "@exception___", "@eventId_"})]
    public async Task WithGivenTemplateParameterNames_ShouldCalculateExpectedPrefixLength(int expectedPrefixLength, params string[] templateParameterNames)
    {
        var actualPrefixLength = ReservedParameterNameResolver.GenerateUniqueIdentifierSuffix(templateParameterNames);
        await Assert.That(actualPrefixLength.Length).IsEqualTo(expectedPrefixLength);
    }
}
