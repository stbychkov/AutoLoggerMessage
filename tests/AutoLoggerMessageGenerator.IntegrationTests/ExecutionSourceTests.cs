using Microsoft.Extensions.Logging;

namespace AutoLoggerMessageGenerator.IntegrationTests;

internal class ExecutionSourceTests
{
    [Test]
    [MethodDataSource(nameof(LogMethodsWithDifferentParameters))]
    public async Task WithAllLogMethods_RequestShouldBeForwardedToLoggerMessageSourceGenerator(Action<ILogger>[] logCalls)
    {
        ILogger logger = LoggerFactory.Create(c =>
            c.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace)
        ).CreateLogger<ExecutionSourceTests>();

        var proxy = DispatchProxyExecutionVerificationDecorator<ILogger>.Decorate(logger, methodName => methodName == "Log");

        foreach (var logCall in logCalls)
            logCall((ILogger) proxy);

        await Assert.That(proxy.ExecutionsWithoutGenerator).IsEmpty();
        await Assert.That(proxy.ExecutionsFromGenerator.Count).IsEqualTo(logCalls.Length);
    }

    [Test]
    public async Task WithMoreThan6Parameters_RequestShouldBeForwardedToOriginalImplementation()
    {
        ILogger logger = LoggerFactory.Create(c =>
            c.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace)
        ).CreateLogger<ExecutionSourceTests>();

        var proxy = DispatchProxyExecutionVerificationDecorator<ILogger>.Decorate(logger, methodName => methodName == "Log");

        const string messageWith7Parameters = "Event received: {arg1} {arg2} {arg3} {arg4} {arg5} {arg6} {arg7}";
        var logCalls = new Action<ILogger>[]
        {
            l => l.LogTrace(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            l => l.LogDebug(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            l => l.LogInformation(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            l => l.LogWarning(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            l => l.LogError(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            l => l.LogCritical(messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7),
            l => l.Log(LogLevel.Information, messageWith7Parameters, 1, 2, 3, 4, 5, 6, 7)
        };

        foreach (var logCall in logCalls)
            logCall((ILogger) proxy);

        await Assert.That(proxy.ExecutionsFromGenerator).IsEmpty();
        await Assert.That(proxy.ExecutionsWithoutGenerator.Count).IsEqualTo(logCalls.Length);
        await Assert.That(proxy.ExecutionsWithoutGenerator.Where(c => !c.Contains("LoggerExtensions.Log"))).IsEmpty();
    }

    public static IEnumerable<Func<Action<ILogger>[]>> LogMethodsWithDifferentParameters()
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
            () =>
            [
                logger => logger.LogTrace(messageWithoutParameters),
                logger => logger.LogDebug(messageWithoutParameters),
                logger => logger.LogInformation(messageWithoutParameters),
                logger => logger.LogWarning(messageWithoutParameters),
                logger => logger.LogError(messageWithoutParameters),
                logger => logger.LogCritical(messageWithoutParameters),
                logger => logger.Log(LogLevel.Information, messageWithoutParameters)
            ],
            () =>
            [
                logger => logger.LogTrace(messageWith1Parameters, 1),
                logger => logger.LogDebug(messageWith1Parameters, 1),
                logger => logger.LogInformation(messageWith1Parameters, 1),
                logger => logger.LogWarning(messageWith1Parameters, 1),
                logger => logger.LogError(messageWith1Parameters, 1),
                logger => logger.LogCritical(messageWith1Parameters, 1),
                logger => logger.Log(LogLevel.Information, messageWith1Parameters, 1)
            ],
            () =>
            [
                logger => logger.LogTrace(messageWith2Parameters, 1, 2),
                logger => logger.LogDebug(messageWith2Parameters, 1, 2),
                logger => logger.LogInformation(messageWith2Parameters, 1, 2),
                logger => logger.LogWarning(messageWith2Parameters, 1, 2),
                logger => logger.LogError(messageWith2Parameters, 1, 2),
                logger => logger.LogCritical(messageWith2Parameters, 1, 2),
                logger => logger.Log(LogLevel.Information, messageWith2Parameters, 1, 2)
            ],
            () =>
            [
                logger => logger.LogTrace(messageWith3Parameters, 1, 2, 3),
                logger => logger.LogDebug(messageWith3Parameters, 1, 2, 3),
                logger => logger.LogInformation(messageWith3Parameters, 1, 2, 3),
                logger => logger.LogWarning(messageWith3Parameters, 1, 2, 3),
                logger => logger.LogError(messageWith3Parameters, 1, 2, 3),
                logger => logger.LogCritical(messageWith3Parameters, 1, 2, 3),
                logger => logger.Log(LogLevel.Information, messageWith3Parameters, 1, 2, 3)
            ],
            () =>
            [
                logger => logger.LogTrace(messageWith4Parameters, 1, 2, 3, 4),
                logger => logger.LogDebug(messageWith4Parameters, 1, 2, 3, 4),
                logger => logger.LogInformation(messageWith4Parameters, 1, 2, 3, 4),
                logger => logger.LogWarning(messageWith4Parameters, 1, 2, 3, 4),
                logger => logger.LogError(messageWith4Parameters, 1, 2, 3, 4),
                logger => logger.LogCritical(messageWith4Parameters, 1, 2, 3, 4),
                logger => logger.Log(LogLevel.Information, messageWith4Parameters, 1, 2, 3, 4)
            ],
            () =>
            [
                logger => logger.LogTrace(messageWith5Parameters, 1, 2, 3, 4, 5),
                logger => logger.LogDebug(messageWith5Parameters, 1, 2, 3, 4, 5),
                logger => logger.LogInformation(messageWith5Parameters, 1, 2, 3, 4, 5),
                logger => logger.LogWarning(messageWith5Parameters, 1, 2, 3, 4, 5),
                logger => logger.LogError(messageWith5Parameters, 1, 2, 3, 4, 5),
                logger => logger.LogCritical(messageWith5Parameters, 1, 2, 3, 4, 5),
                logger => logger.Log(LogLevel.Information, messageWith5Parameters, 1, 2, 3, 4, 5)
            ],
            () =>
            [
                logger => logger.LogTrace(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
                logger => logger.LogDebug(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
                logger => logger.LogInformation(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
                logger => logger.LogWarning(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
                logger => logger.LogError(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
                logger => logger.LogCritical(messageWith6Parameters, 1, 2, 3, 4, 5, 6),
                logger => logger.Log(LogLevel.Information, messageWith6Parameters, 1, 2, 3, 4, 5, 6)
            ]
        ];
    }
}
