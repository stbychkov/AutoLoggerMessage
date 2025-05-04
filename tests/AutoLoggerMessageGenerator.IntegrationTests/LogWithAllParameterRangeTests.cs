using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.IntegrationTests;

internal class LogWithAllParameterRangeTests
{
    [Test]
    [MethodDataSource(nameof(LogMethodsWithDifferentParameters))]
    public async Task WithAllLogMethods_RequestShouldBeForwardedToLoggerMessageSourceGenerator(
        Action<ILogger> logCall)
    {
        ILogger logger = LoggerFactory.Create(c =>
            c.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace)
        ).CreateLogger<LogWithAllParameterRangeTests>();

        var proxy = DispatchProxyExecutionVerificationDecorator<ILogger>.Decorate(logger,
            Constants.LoggerMessageGeneratorName, methodName => methodName == "Log");

        logCall((ILogger) proxy);

        await Assert.That(proxy.ExecutionsWithoutGenerator).IsEmpty();
        await Assert.That(proxy.ExecutionsFromGenerator.Count).IsEqualTo(1);
    }

    [Test]
    [MethodDataSource(nameof(LogMethodsWithUnsupportedParametersCount))]
    public async Task WithUnsupportedParametersCount_RequestShouldBeForwardedToOriginalImplementation(Action<ILogger> logCall)
    {
        ILogger logger = LoggerFactory.Create(c =>
            c.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace)
        ).CreateLogger<LogWithAllParameterRangeTests>();

        var proxy = DispatchProxyExecutionVerificationDecorator<ILogger>.Decorate(logger,
            Constants.LoggerMessageGeneratorName, methodName => methodName == "Log");

        logCall((ILogger) proxy);

        await Assert.That(proxy.ExecutionsFromGenerator).IsEmpty();
        await Assert.That(proxy.ExecutionsWithoutGenerator.Count).IsEqualTo(1);
        await Assert.That(proxy.ExecutionsWithoutGenerator.Where(c => !c.Contains("LoggerExtensions.Log"))).IsEmpty();
    }

    public static IEnumerable<Func<Action<ILogger>>> LogMethodsWithDifferentParameters()
    {
        const string messageWithoutParameters = "Event received";
        const string messageWith1Parameters = "Event received: {arg1}";
        const string messageWith2Parameters = "Event received: {arg1} {arg2}";
        const string messageWith3Parameters = "Event received: {arg1} {arg2} {arg3}";
        const string messageWith4Parameters = "Event received: {arg1} {arg2} {arg3} {arg4}";
        const string messageWith5Parameters = "Event received: {arg1} {arg2} {arg3} {arg4} {arg5}";
        const string messageWith6Parameters = "Event received: {arg1} {arg2} {arg3} {arg4} {arg5} {arg6}";

        return
        [
            () => l => l.LogTrace(messageWithoutParameters),
            () => l => l.LogDebug(messageWithoutParameters),
            () => l => l.LogInformation(messageWithoutParameters),
            () => l => l.LogWarning(messageWithoutParameters),
            () => l => l.LogError(messageWithoutParameters),
            () => l => l.LogCritical(messageWithoutParameters),
            () => l => l.Log(LogLevel.Information, messageWithoutParameters),

            () => l => l.LogTrace(messageWith1Parameters, 1),
            () => l => l.LogDebug(messageWith1Parameters, 1),
            () => l => l.LogInformation(messageWith1Parameters, 1),
            () => l => l.LogWarning(messageWith1Parameters, 1),
            () => l => l.LogError(messageWith1Parameters, 1),
            () => l => l.LogCritical(messageWith1Parameters, 1),
            () => l => l.Log(LogLevel.Information, messageWith1Parameters, 1),

            () => l => l.LogTrace(messageWith2Parameters, 1, 2),
            () => l => l.LogDebug(messageWith2Parameters, 1, 2),
            () => l => l.LogInformation(messageWith2Parameters, 1, 2),
            () => l => l.LogWarning(messageWith2Parameters, 1, 2),
            () => l => l.LogError(messageWith2Parameters, 1, 2),
            () => l => l.LogCritical(messageWith2Parameters, 1, 2),
            () => l => l.Log(LogLevel.Information, messageWith2Parameters, 1, 2),

            () => l => l.LogTrace(messageWith3Parameters, 1, 2, 3),
            () => l => l.LogDebug(messageWith3Parameters, 1, 2, 3),
            () => l => l.LogInformation(messageWith3Parameters, 1, 2, 3),
            () => l => l.LogWarning(messageWith3Parameters, 1, 2, 3),
            () => l => l.LogError(messageWith3Parameters, 1, 2, 3),
            () => l => l.LogCritical(messageWith3Parameters, 1, 2, 3),
            () => l => l.Log(LogLevel.Information, messageWith3Parameters, 1, 2, 3),

            () => l => l.LogTrace(messageWith4Parameters, 1, 2, 3, 4),
            () => l => l.LogDebug(messageWith4Parameters, 1, 2, 3, 4),
            () => l => l.LogInformation(messageWith4Parameters, 1, 2, 3, 4),
            () => l => l.LogWarning(messageWith4Parameters, 1, 2, 3, 4),
            () => l => l.LogError(messageWith4Parameters, 1, 2, 3, 4),
            () => l => l.LogCritical(messageWith4Parameters, 1, 2, 3, 4),
            () => l => l.Log(LogLevel.Information, messageWith4Parameters, 1, 2, 3, 4),

            () => l => l.LogTrace(messageWith5Parameters, 1, 2, 3, 4, 5),
            () => l => l.LogDebug(messageWith5Parameters, 1, 2, 3, 4, 5),
            () => l => l.LogInformation(messageWith5Parameters, 1, 2, 3, 4, 5),
            () => l => l.LogWarning(messageWith5Parameters, 1, 2, 3, 4, 5),
            () => l => l.LogError(messageWith5Parameters, 1, 2, 3, 4, 5),
            () => l => l.LogCritical(messageWith5Parameters, 1, 2, 3, 4, 5),
            () => l => l.Log(LogLevel.Information, messageWith5Parameters, 1, 2, 3, 4, 5),

            () => l => l.LogTrace(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
            () => l => l.LogDebug(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
            () => l => l.LogInformation(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
            () => l => l.LogWarning(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
            () => l => l.LogError(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
            () => l => l.LogCritical(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
            () => l => l.Log(LogLevel.Information, messageWith6Parameters, 1, 2, 3, 4, 5, 6)
        ];
    }

    public static IEnumerable<Func<Action<ILogger>>> LogMethodsWithUnsupportedParametersCount()
    {
        const string messageWith7Parameters = "Event received: {arg1} {arg2} {arg3} {arg4} {arg5} {arg6} {arg7}";
        return
        [
            () => l => l.LogTrace(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            () => l => l.LogDebug(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            () => l => l.LogInformation(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            () => l => l.LogWarning(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            () => l => l.LogError(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            () => l => l.LogCritical(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            () => l => l.Log(LogLevel.Information, messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7)
        ];
    }
}
