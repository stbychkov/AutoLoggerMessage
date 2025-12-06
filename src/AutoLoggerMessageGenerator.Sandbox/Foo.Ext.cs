using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.Sandbox;

public partial class Foo
{
    public void Zoo()
    {
        var act = () =>
        {
            logger.LogWarning("HELLLLLO");
        };
        logger.LogInformation("Bar {parameter}", 123);
    }
}
