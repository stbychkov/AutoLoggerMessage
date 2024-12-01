using System;
using Microsoft.Extensions.Logging;

public class CustomNullLogger<T> : ILogger
{
    private int _counter = 0;
    public static readonly CustomNullLogger<T> Instance = new();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        _counter++;
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull =>
        NullScope.Instance;
    
    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose() { }
    }
}
