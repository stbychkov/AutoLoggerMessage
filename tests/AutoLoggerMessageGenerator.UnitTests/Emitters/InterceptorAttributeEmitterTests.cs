using AutoLoggerMessageGenerator.Emitters;
using AutoLoggerMessageGenerator.UnitTests.Scrubbers;
using AutoLoggerMessageGenerator.UnitTests.Utilities;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

internal class InterceptorAttributeEmitterTests
{
    [Test]
    public async Task Emit_ShouldGenerateValidInterceptorAttribute()
    {
        var sourceCode = InterceptorAttributeEmitter.Emit();
        var configuration = InterceptorConfigurationUtilities.GetInterceptorConfiguration();

        await Verify(sourceCode)
            .UseTextForParameters(configuration)
            .AddCodeGeneratedAttributeScrubber();
    }
}
