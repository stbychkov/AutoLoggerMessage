using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.UnitTests.Scrubbers;
using AutoLoggerMessageGenerator.UnitTests.Utilities;
using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

internal class LoggerInterceptorsEmitterTests
{
    [Test]
    public async Task Emit_ShouldGenerateValidLoggingExtensionsAttribute()
    {
        ImmutableArray<LogMessageCall> logCalls =
        [
            new LogMessageCall(
                Id: Guid.NewGuid(),
                Location: MockLogCallLocationBuilder.Build("file", 1, 11),
                Namespace: "namespace1",
                ClassName: "class1",
                MethodName: "name1",
                LogLevel: "Information",
                Message: "Message1",
                Parameters: [new CallParameter("string", MessageParameterName, CallParameterType.Message)]
            ),
            new LogMessageCall(
                Id: Guid.NewGuid(),
                Location: MockLogCallLocationBuilder.Build("file2", 2, 22),
                Namespace: "namespace2",
                ClassName: "class2",
                MethodName: "name2",
                LogLevel: "Warning",
                Message: "Message2",
                Parameters: [
                    new CallParameter("string", MessageParameterName, CallParameterType.Message),
                    new CallParameter("int", "@intParam", CallParameterType.Others)
                ]
            ),
            new LogMessageCall(
                Id: Guid.NewGuid(),
                Location: MockLogCallLocationBuilder.Build("file3", 3, 33),
                Namespace: "namespace3",
                ClassName: "class3",
                MethodName: "name3",
                LogLevel: "Error",
                Message: "Message3",
                Parameters: [
                    new CallParameter("string", MessageParameterName, CallParameterType.Message),
                    new CallParameter("int", "@intParam", CallParameterType.Others),
                    new CallParameter("bool", "@boolParam", CallParameterType.Others),
                    new CallParameter("SomeClass", "@objectParam", CallParameterType.Others, true)
                ]
            ),
        ];

        var sourceCode = LoggerInterceptorsEmitter.Emit(logCalls);
        await Verify(sourceCode).AddCodeGeneratedAttributeScrubber()
            .UseTextForParameters(RoslynConfigurationUtilities.GetRoslynVersion());
    }
}
