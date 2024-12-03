using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Models;
using Microsoft.CodeAnalysis;
using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

public class LoggerInterceptorsEmitterTests
{
    [Fact]
    public async Task Emit_ShouldGenerateValidLoggingExtensionsAttribute()
    {
        ImmutableArray<LogCall> logCalls =
        [
            new LogCall(
                Id: Guid.NewGuid(),
                Location: new LogCallLocation("file", 1, 11, Location.None),
                Namespace: "namespace1",
                ClassName: "class1",
                Name: "name1",
                LogLevel: "Information",
                Message: "Message1",
                Parameters: [new LogCallParameter("string", MessageParameterName, LogCallParameterType.Message)]
            ),
            new LogCall(
                Id: Guid.NewGuid(),
                Location: new LogCallLocation("file2", 2, 22, Location.None),
                Namespace: "namespace2",
                ClassName: "class2",
                Name: "name2",
                LogLevel: "Warning",
                Message: "Message2",
                Parameters: [
                    new LogCallParameter("string", MessageParameterName, LogCallParameterType.Message),
                    new LogCallParameter("int", "@intParam", LogCallParameterType.Others)
                ]
            ),
            new LogCall(
                Id: Guid.NewGuid(),
                Location: new LogCallLocation("file3", 3, 33, Location.None),
                Namespace: "namespace3",
                ClassName: "class3",
                Name: "name3",
                LogLevel: "Error",
                Message: "Message3",
                Parameters: [
                    new LogCallParameter("string", MessageParameterName, LogCallParameterType.Message),
                    new LogCallParameter("int", "@intParam", LogCallParameterType.Others),
                    new LogCallParameter("bool", "@boolParam", LogCallParameterType.Others),
                    new LogCallParameter("SomeClass", "@objectParam", LogCallParameterType.Others, true)
                ]
            ),
        ];

        var sourceCode = LoggerInterceptorsEmitter.Emit(logCalls);
        await Verify(sourceCode);
    }
}
