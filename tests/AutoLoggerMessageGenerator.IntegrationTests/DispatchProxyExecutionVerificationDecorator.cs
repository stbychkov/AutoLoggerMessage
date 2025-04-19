using System.Reflection;

namespace AutoLoggerMessageGenerator.IntegrationTests;

public class DispatchProxyExecutionVerificationDecorator<T> : DispatchProxy
{
    private T? Target { get; set; }

    private Func<string, bool>? MethodFilter { get; set; }

    private readonly List<string> _executionsFromGenerator = [];
    private readonly List<string> _executionsWithoutGenerator = [];

    public IReadOnlyList<string> ExecutionsFromGenerator => _executionsFromGenerator.AsReadOnly();
    public IReadOnlyList<string> ExecutionsWithoutGenerator => _executionsWithoutGenerator.AsReadOnly();

    public static DispatchProxyExecutionVerificationDecorator<T> Decorate(T target, Func<string, bool>? methodFilter = default)
    {
        if (Create<T, DispatchProxyExecutionVerificationDecorator<T>>() is not DispatchProxyExecutionVerificationDecorator<T> proxy)
            throw new InvalidOperationException($"Unable to create DispatchProxyExecutionVerificationDecorator for {typeof(T).FullName}");

        proxy.Target = target;
        proxy.MethodFilter = methodFilter;
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (MethodFilter == default || (targetMethod?.Name is not null && MethodFilter(targetMethod.Name)))
            CaptureExecutionCall();

        return targetMethod?.Invoke(Target, args);
    }

    private void CaptureExecutionCall()
    {
        var stackTrace = Environment.StackTrace;
        var callFromGenerator = stackTrace.Contains("LoggerMessage.g.cs");

        var executionList = callFromGenerator ? _executionsFromGenerator : _executionsWithoutGenerator;
        executionList.Add(stackTrace);
    }
}
