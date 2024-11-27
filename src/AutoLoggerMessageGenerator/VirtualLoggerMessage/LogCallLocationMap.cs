using System;
using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AutoLoggerMessageGenerator.VirtualLoggerMessage;

internal static class LogCallLocationMap
{
    private const string LogCallMappingLocationFlag = "// <LogCallMappingLocation>: ";

    public static string CreateMapping(LogCall logCall) =>
        $"{LogCallMappingLocationFlag}{logCall.Id}";

    public static bool TryMapBack(SourceText syntaxTree, Location currentLocation, out Guid logCallId)
    {
        var subText = syntaxTree.GetSubText(new TextSpan(0, currentLocation.SourceSpan.Start - 1));

        for (var lineIndex = subText.Lines.Count - 1; lineIndex >= 0; lineIndex--)
        {
            var line = subText.Lines[lineIndex].ToString().TrimStart();
            if (!line.StartsWith(LogCallMappingLocationFlag)) continue;

            return Guid.TryParse(line.Substring(LogCallMappingLocationFlag.Length), out logCallId);
        }

        return false;
    }
}
