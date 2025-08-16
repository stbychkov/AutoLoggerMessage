using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Models;
using AutoLoggerMessageGenerator.UnitTests.Scrubbers;
using AutoLoggerMessageGenerator.UnitTests.Utilities;
using static AutoLoggerMessageGenerator.Constants;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

internal class LoggerScopesEmitterTests
{
    [Test]
    public async Task Emit_WithGivenConfiguration_ShouldGenerateValidLoggerDefineScopedFunctors()
    {
        ImmutableArray<LoggerScopeCall> loggerScopes =
        [
            new(
                Location: MockLogCallLocationBuilder.Build("file", 1, 11),
                Namespace: "namespace1",
                ClassName: "class1",
                MethodName: "name1",
                Message: "Message1",
                Parameters: [new CallParameter("string", MessageParameterName, CallParameterType.Message)]
            ),
            new(
                Location: MockLogCallLocationBuilder.Build("file2", 2, 22),
                Namespace: "namespace2",
                ClassName: "class2",
                MethodName: "name2",
                Message: "Message2",
                Parameters: [
                    new CallParameter("string", MessageParameterName, CallParameterType.Message),
                    new CallParameter("int", "@intParam", CallParameterType.Others)
                ]
            ),
            new(
                Location: MockLogCallLocationBuilder.Build("file3", 3, 33),
                Namespace: "namespace3",
                ClassName: "class3",
                MethodName: "name3",
                Message: "Message3",
                Parameters: [
                    new CallParameter("string", MessageParameterName, CallParameterType.Message),
                    new CallParameter("int", "@intParam", CallParameterType.Others),
                    new CallParameter("bool", "@boolParam", CallParameterType.Others),
                    new CallParameter("SomeClass", "@objectParam", CallParameterType.Others, true)
                ]
            ),
        ];

        var sourceCode = LoggerScopesEmitter.Emit(loggerScopes);
        await Verify(sourceCode).AddCodeGeneratedAttributeScrubber()
            .UseTextForParameters(RoslynConfigurationUtilities.GetRoslynVersion());
    }
}
