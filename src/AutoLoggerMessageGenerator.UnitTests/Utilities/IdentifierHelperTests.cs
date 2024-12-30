using AutoLoggerMessageGenerator.Utilities;

namespace AutoLoggerMessageGenerator.UnitTests.Utilities;

internal class IdentifierHelperTests
{
    [Test]
    [Arguments("ValidName", "ValidName")]
    [Arguments("_Valid__Name___", "_Valid__Name___")]
    [Arguments("🔥Invalid🔥Emoji🔥", "__Invalid__Emoji__")]
    [Arguments("123Name", "_123Name")]
    [Arguments("Name#With$Special%Chars", "Name_With_Special_Chars")]
    public async Task ToValidCSharpMethodName_ShouldSanitizeCorrectly(string input, string expected)
    {
        var result = IdentifierHelper.ToValidCSharpMethodName(input);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public async Task ToValidCSharpMethodName_ShouldThrow_WhenInputIsNull(string? input)
    {
        string Action() => IdentifierHelper.ToValidCSharpMethodName(input);
        await Assert.That(Action).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    [Arguments("validName", true)]
    [Arguments("_validName", true)]
    [Arguments("ValidName123", true)]
    [Arguments("a", true)]
    [Arguments("invalid name", false)]
    [Arguments("123Invalid", false)]
    [Arguments("invalid-name", false)]
    [Arguments("e.invalid", false)]
    [Arguments("i🔥nvalid", false)]
    [Arguments("", false)]
    [Arguments(null, false)]
    public async Task IsValidCSharpParameterName_WithGivenParameterName_ShouldReturnTheGivenResult(string? parameterName,
        bool isValid)
    {
        var result = IdentifierHelper.IsValidCSharpParameterName(parameterName);
        await Assert.That(result).IsEqualTo(isValid);
    }
}
