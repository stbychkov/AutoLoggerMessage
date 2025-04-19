using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.UnitTests.Scrubbers;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

internal class LoggerExtensionsEmitterTests
{
    [Test]
    public async Task Emit_ShouldGenerateValidLoggingExtensionsAttribute()
    {
        var sourceCode = LoggerExtensionsEmitter.Emit();
        await Verify(sourceCode).AddCodeGeneratedAttributeScrubber();
    }
}
