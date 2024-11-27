using System.Collections.Immutable;
using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.Models;

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
                Location: new LogCallLocation("file", 1, 11, default), 
                Namespace: "namespace1", 
                ClassName: "class1", 
                Name: "name1",
                LogLevel: "Information",
                Message: "Message1",
                Parameters: []
            ),
            new LogCall(
                Id: Guid.NewGuid(), 
                Location: new LogCallLocation("file2", 2, 22, default), 
                Namespace: "namespace2", 
                ClassName: "class2", 
                Name: "name2",
                LogLevel: "Warning",
                Message: "Message2",
                Parameters: [new LogCallParameter("int", "intParam", false)]
            ),
            new LogCall(
                Id: Guid.NewGuid(), 
                Location: new LogCallLocation("file3", 3, 33, default), 
                Namespace: "namespace3", 
                ClassName: "class3", 
                Name: "name3",
                LogLevel: "Error",
                Message: "Message3",
                Parameters: [
                    new LogCallParameter("int", "intParam", false),
                    new LogCallParameter("bool", "boolParam", false),
                    new LogCallParameter("SomeClass", "objectParam", true)
                ]
            ),
        ];
        
        var sourceCode = LoggerInterceptorsEmitter.Emit(logCalls);
        await Verify(sourceCode);
    }
}
