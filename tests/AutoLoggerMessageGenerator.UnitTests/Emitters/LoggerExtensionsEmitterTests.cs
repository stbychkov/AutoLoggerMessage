using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.UnitTests.Scrubbers;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

internal class GenericLoggerExtensionsEmitterTests
{
    [Test]
    public async Task Emit_ShouldGenerateValidLoggingExtensionsOverrides()
    {
        var sourceCode = GenericLoggerExtensionsEmitter.Emit();
        await Verify(sourceCode).AddCodeGeneratedAttributeScrubber();
    }
}
