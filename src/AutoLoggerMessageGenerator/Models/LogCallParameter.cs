namespace AutoLoggerMessageGenerator.Models;

internal record struct LogCallParameter
(
    string NativeType,
    string Name,
    LogCallParameterType Type,
    bool HasPropertiesToLog = false
);
