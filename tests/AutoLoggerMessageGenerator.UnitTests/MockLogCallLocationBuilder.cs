using System.Text;
using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.UnitTests;

internal static class MockLogCallLocationBuilder
{
    internal static LogCallLocation Build(string filePath, int line, int character)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        const int version = -1;
        var location = $"{filePath}({line},{character})";
        var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(location));
        var interceptableLocationSyntax = $"""[FakeInterceptableLocation({version}, "{data}")]""";

        return new LogCallLocation(filePath, line, character, interceptableLocationSyntax, Location.None);
    }
}
