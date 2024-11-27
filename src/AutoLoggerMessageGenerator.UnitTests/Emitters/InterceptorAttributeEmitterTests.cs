using AutoLoggerMessageGenerator.Emitters;

namespace AutoLoggerMessageGenerator.UnitTests.Emitters;

public class InterceptorAttributeEmitterTests
{
    [Fact]
    public async Task Emit_ShouldGenerateValidInterceptorAttribute()
    {
        var sourceCode = InterceptorAttributeEmitter.Emit();

        await Verify(sourceCode);
    }  
}
