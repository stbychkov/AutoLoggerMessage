using System.Diagnostics;

namespace AutoLoggerMessageGenerator.Benchmarks;

internal class CommandRunner(ProcessPriorityClass priorityClass)
{
    public async Task RunAsync(string command, string? args = null, CancellationToken cancellationToken = default)
    {
        var process = new Process();

        process.StartInfo = new ProcessStartInfo(command, args ?? string.Empty)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        try
        {
            process.PriorityClass = priorityClass;
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to set priority class");
        }
        process.EnableRaisingEvents = true;

        process.Start();
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
            throw new InvalidOperationException("Command execution failed. Check execution logs for details");
    }
}
