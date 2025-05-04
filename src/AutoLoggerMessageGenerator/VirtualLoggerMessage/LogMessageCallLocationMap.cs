using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AutoLoggerMessageGenerator.VirtualLoggerMessage;

internal static class LogMessageCallLocationMap
{
    private const string LogMessageCallMappingLocationFlag = "// <LogMessageCallMappingLocation>: ";

    public static string CreateMapping(LogMessageCall logMessageCall) =>
        $"{LogMessageCallMappingLocationFlag}{logMessageCall.Id}";

    public static bool TryMapBack(SourceText syntaxTree, Location currentLocation, out Guid logCallId)
    {
        logCallId = default;

        var subText = syntaxTree.GetSubText(new TextSpan(0, currentLocation.SourceSpan.Start - 1));

        for (var lineIndex = subText.Lines.Count - 1; lineIndex >= 0; lineIndex--)
        {
            var line = subText.Lines[lineIndex].ToString().TrimStart();
            if (!line.StartsWith(LogMessageCallMappingLocationFlag)) continue;

            return Guid.TryParse(line.Substring(LogMessageCallMappingLocationFlag.Length), out logCallId);
        }

        return false;
    }
}
