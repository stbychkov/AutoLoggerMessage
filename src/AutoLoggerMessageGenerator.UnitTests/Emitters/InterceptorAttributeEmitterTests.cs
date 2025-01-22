using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.UnitTests.Scrubbers;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

internal class InterceptorAttributeEmitterTests
{
    [Test]
    public async Task Emit_ShouldGenerateValidInterceptorAttribute()
    {
        var sourceCode = InterceptorAttributeEmitter.Emit();
        await Verify(sourceCode).AddCodeGeneratedAttributeScrubber();
    }
}
