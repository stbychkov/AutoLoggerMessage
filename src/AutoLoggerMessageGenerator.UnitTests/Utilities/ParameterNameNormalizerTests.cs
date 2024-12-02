using AutoLoggerMessageGenerator.Utilities;
using FluentAssertions;

using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.UnitTests.Utilities;

public class ParameterNameNormalizerTests
{
    [Theory]
    [InlineData(MessageArgumentName)]
    [InlineData(EventIdArgumentName)]
    [InlineData(ExceptionArgumentName)]
    public void Normalize_WithReservedName_ShouldReturnNormalizedName(string parameterName)
    {
        var sut = new ParameterNameNormalizer();

        var result1 = sut.Normalize(parameterName);
        var result2 = sut.Normalize(parameterName);
        var result3 = sut.Normalize(parameterName);

        result1.Should().Be($"{parameterName}1");
        result2.Should().Be($"{parameterName}2");
        result3.Should().Be($"{parameterName}3");
    }
    
    [Fact]
    public void Normalize_WithNotReservedName_ShouldReturnParameterName()
    {
        var sut = new ParameterNameNormalizer();
        const string parameterName = "someParameterName";

        var result1 = sut.Normalize(parameterName);
        var result2 = sut.Normalize(parameterName);
        var result3 = sut.Normalize(parameterName);

        result1.Should().Be($"@{parameterName}");
        result2.Should().Be($"@{parameterName}");
        result3.Should().Be($"@{parameterName}");
    }
}
