using AutoLoggerMessageGenerator.Utilities;
using FluentAssertions;

namespace AutoLoggerMessageGenerator.UnitTests.Utilities;

public class IdentifierHelperTests
{
    [Theory]
    [InlineData("ValidName", "ValidName")]
    [InlineData("_Valid__Name___", "_Valid__Name___")]
    [InlineData("🔥Invalid🔥Emoji🔥", "__Invalid__Emoji__")]
    [InlineData("123Name", "_123Name")]
    [InlineData("Name#With$Special%Chars", "Name_With_Special_Chars")] 
    public void ToValidCSharpMethodName_ShouldSanitizeCorrectly(string input, string expected)
    {
        var result = IdentifierHelper.ToValidCSharpMethodName(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ToValidCSharpMethodName_ShouldThrow_WhenInputIsNull(string? input)
    {
        var action = () => IdentifierHelper.ToValidCSharpMethodName(input);
        action.Should().ThrowExactly<ArgumentNullException>();
    }
}
