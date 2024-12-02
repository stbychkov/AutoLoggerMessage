using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.VirtualLoggerMessage;
using Microsoft.CodeAnalysis;

namespace AutoLoggerMessageGenerator.Diagnostics;

internal class DiagnosticReporter
{
    private readonly SourceProductionContext _context;
    private readonly Dictionary<Guid, LogCall> _logCallsIndex;

    public DiagnosticReporter(SourceProductionContext context, IEnumerable<LogCall> logCalls)
    {
        _context = context;
        _logCallsIndex = logCalls.ToDictionary(c => c.Id);
    }

    public void Report(Diagnostic diagnostic)
    {
        // All imported source generators use SimpleDiagnostic which is internal and doesn't support cloning with message arguments
        var messageArgs = typeof(Diagnostic).GetProperty("Arguments", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(diagnostic) as object[];

        var sourceText = diagnostic.Location.SourceTree?.GetText(_context.CancellationToken);
        var location = sourceText is not null &&
                       LogCallLocationMap.TryMapBack(sourceText, diagnostic.Location, out var logCallId) &&
                       _logCallsIndex.TryGetValue(logCallId, out var logCall)
                       ? logCall.Location.Context
                       : Location.None;

        _context.ReportDiagnostic(Diagnostic.Create(diagnostic.Descriptor, location, messageArgs));
    }
}
