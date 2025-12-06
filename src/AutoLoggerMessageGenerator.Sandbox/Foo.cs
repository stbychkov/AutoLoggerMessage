using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.Sandbox;

public partial class Foo(ILogger<Foo> logger)
{
    public void Bar()
    {
        logger.LogInformation("Bar {parameter}", 123);
    }
}
