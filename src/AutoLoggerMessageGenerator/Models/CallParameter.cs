namespace AutoLoggerMessageGenerator.Models;

internal record struct CallParameter
(
    string NativeType,
    string Name,
    CallParameterType Type,
    bool HasPropertiesToLog = false
);
