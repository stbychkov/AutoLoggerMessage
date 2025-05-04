
using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.IntegrationTests;

internal class BeginScopeWithAllParameterRangeTests
{
    [Test]
    [MethodDataSource(nameof(BeginScopeMethodsWithDifferentParameters))]
    public async Task WithAllLogMethods_RequestShouldBeForwardedToLoggerMessageSourceGenerator(Func<ILogger, IDisposable?> beginScopeCall)
    {
        ILogger logger = LoggerFactory.Create(c =>
            c.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
            })
        ).CreateLogger<BeginScopeWithAllParameterRangeTests>();

        var proxy = DispatchProxyExecutionVerificationDecorator<ILogger>.Decorate(logger,
            Constants.LoggerScopesGeneratorName, methodName => methodName == "BeginScope");

        using var _ = beginScopeCall((ILogger) proxy);
        logger.LogInformation("Hello world");

        await Assert.That(proxy.ExecutionsWithoutGenerator).IsEmpty();
        await Assert.That(proxy.ExecutionsFromGenerator.Count).IsEqualTo(1);
    }

    [Test]
    [MethodDataSource(nameof(BeginScopeMethodsWithUnsupportedParametersCount))]
    public async Task WithUnsupportedParametersCount_RequestShouldBeForwardedToOriginalImplementation(Func<ILogger, IDisposable?> beginScopeCall)
    {
        ILogger logger = LoggerFactory.Create(c =>
            c.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace)
        ).CreateLogger<BeginScopeWithAllParameterRangeTests>();

        var proxy = DispatchProxyExecutionVerificationDecorator<ILogger>.Decorate(logger,
            Constants.LoggerScopesGeneratorName, methodName => methodName == "BeginScope");

        using var _ = beginScopeCall((ILogger) proxy);
        logger.LogInformation("Hello world");

        await Assert.That(proxy.ExecutionsFromGenerator).IsEmpty();
        await Assert.That(proxy.ExecutionsWithoutGenerator.Count).IsEqualTo(1);
    }

    public static IEnumerable<Func<Func<ILogger, IDisposable?>>> BeginScopeMethodsWithDifferentParameters()
    {
        const string messageWith1Parameters = "Message Scope: {arg1}";
        const string messageWith2Parameters = "Message Scope: {arg1} {arg2}";
        const string messageWith3Parameters = "Message Scope: {arg1} {arg2} {arg3}";
        const string messageWith4Parameters = "Message Scope: {arg1} {arg2} {arg3} {arg4}";
        const string messageWith5Parameters = "Message Scope: {arg1} {arg2} {arg3} {arg4} {arg5}";
        const string messageWith6Parameters = "Message Scope: {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}";

        return
        [
            () => l => l.BeginScope(messageWith1Parameters, 1),
            () => l => l.BeginScope(messageWith2Parameters, 1, 2),
            () => l => l.BeginScope(messageWith3Parameters, 1, 2, 3),
            () => l => l.BeginScope(messageWith4Parameters, 1, 2, 3, 4),
            () => l => l.BeginScope(messageWith5Parameters, 1, 2, 3, 4, 5),
            () => l => l.BeginScope(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
        ];
    }

    public static IEnumerable<Func<Func<ILogger, IDisposable?>>> BeginScopeMethodsWithUnsupportedParametersCount()
    {
        const string messageWithoutParameters = "Message Scope";
        const string messageWith7Parameters = "Message Scope: {arg1} {arg2} {arg3} {arg4} {arg5} {arg6} {arg7}";

        return
        [
            () => l => l.BeginScope(messageWithoutParameters),
            () => l => l.BeginScope(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
        ];
    }
}
