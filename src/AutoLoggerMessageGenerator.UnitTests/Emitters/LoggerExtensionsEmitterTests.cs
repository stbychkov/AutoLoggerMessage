using AutoLoggerMessageGenerator.Emitters;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

public class LoggerExtensionsEmitterTests
{
    [Fact]
    public async Task Emit_ShouldGenerateValidLoggingExtensionsAttribute()
    {
        var sourceCode = LoggerExtensionsEmitter.Emit();
        await Verify(sourceCode);
    }
}
