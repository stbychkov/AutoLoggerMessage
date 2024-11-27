using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AutoLoggerMessageGenerator.IntegrationTests;

public class LogPropertiesAttributeTests
{
    private static readonly ILogger Logger = LoggerFactory.Create(c =>
        c.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace)
    ).CreateLogger<LogPropertiesAttributeTests>();

    [Fact]
    public static void AllLogPropertiesHaveToBeLogged()
    {
        IEvent generatorCallCapturedEvent = new GeneratorCallCapturedEvent { Id = Guid.NewGuid() };
        var proxy = DispatchProxyExecutionVerificationDecorator<IEvent>.Decorate(generatorCallCapturedEvent);
        generatorCallCapturedEvent = (IEvent) proxy;

        var propertiesCount = typeof(GeneratorCallCapturedEvent)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Length;

        Logger.LogInformation("Event received: {event}", generatorCallCapturedEvent);
        
        proxy.ExecutionsWithoutGenerator.Should().BeEmpty();
        proxy.ExecutionsFromGenerator.Should().HaveCount(propertiesCount);
    }
}

internal interface IEvent
{
    Guid Id { get; }
    
    string Name { get; }
    
    long Timestamp { get; }
}

internal sealed record GeneratorCallCapturedEvent : IEvent
{
    public required Guid Id { get; init; }
    
    public string Name => nameof(GeneratorCallCapturedEvent);
    
    public long Timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
