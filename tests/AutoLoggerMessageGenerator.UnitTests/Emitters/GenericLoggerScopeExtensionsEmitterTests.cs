using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.UnitTests.Scrubbers;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

internal class GenericLoggerScopeExtensionsEmitterTests
{
    [Test]
    public async Task Emit_ShouldGenerateValidLoggingScopeExtensionsOverrides()
    {
        var sourceCode = GenericLoggerScopeExtensionsEmitter.Emit();
        await Verify(sourceCode).AddCodeGeneratedAttributeScrubber().AutoVerify();
    }
}
