using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.UnitTests.Scrubbers;
using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

internal class LoggerInterceptorsEmitterTests
{
    [Test]
    public async Task Emit_ShouldGenerateValidLoggingExtensionsAttribute()
    {
        ImmutableArray<LogCall> logCalls =
        [
            new LogCall(
                Id: Guid.NewGuid(),
                Location: MockLogCallLocationBuilder.Build("file", 1, 11),
                Namespace: "namespace1",
                ClassName: "class1",
                Name: "name1",
                LogLevel: "Information",
                Message: "Message1",
                Parameters: [new LogCallParameter("string", MessageParameterName, LogCallParameterType.Message)]
            ),
            new LogCall(
                Id: Guid.NewGuid(),
                Location: MockLogCallLocationBuilder.Build("file2", 2, 22),
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
                Location: MockLogCallLocationBuilder.Build("file3", 3, 33),
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
        await Verify(sourceCode).AddCodeGeneratedAttributeScrubber();
    }
}
