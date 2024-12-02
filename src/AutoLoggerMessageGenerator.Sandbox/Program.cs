using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => 
    builder.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace)
);
var logger = loggerFactory.CreateLogger<Program>();

logger.LogTrace("Log message without parameters");
logger.LogDebug("Log message with parameters: {Param1}, {Param2}", 42, "Hello, World!");
logger.LogInformation("Log message with 6 parameters: {arg1}, {arg2}, {arg3}, {arg4}, {arg5} {arg6}", 1, 2, 3, 4, 5, 6);
logger.LogWarning(new EventId(42, "Event1"), "Event1 happened");
logger.LogError(new EventId(42, "Event1"), new Exception("Event1 error"), "Event1 happened");
logger.LogCritical(new EventId(42, "Event2"), new Exception("Event2 error"), "Event2 happened {arg1}", new EventData(123, "Event details"));

internal record EventData(int Id, string Name);
