namespace AutoLoggerMessageGenerator.Models;

internal record struct LogCallParameter
(
    string Type,
    string Name,
    bool HasPropertiesToLog = false
);
