using System.Reflection;
using System.Runtime.Versioning;

namespace AutoLoggerMessageGenerator.Benchmarks;

public static class TargetFrameworkMonikerDetector
{
    public static string Detect()
    {
        var frameworkName = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<TargetFrameworkAttribute>()?
            .FrameworkName;

        if (string.IsNullOrEmpty(frameworkName))
            throw new InvalidOperationException("Failed to detect target framework moniker");

        return frameworkName switch
        {
            ".NETFramework,Version=v4.6.1" => "net461",
            ".NETFramework,Version=v4.6.2" => "net462",
            ".NETFramework,Version=v4.7" => "net47",
            ".NETFramework,Version=v4.7.1" => "net471",
            ".NETFramework,Version=v4.7.2" => "net472",
            ".NETFramework,Version=v4.8" => "net48",
            ".NETFramework,Version=v4.8.1" => "net481",
            ".NETCoreApp,Version=v2.0" => "netcoreapp2.0",
            ".NETCoreApp,Version=v2.1" => "netcoreapp2.1",
            ".NETCoreApp,Version=v2.2" => "netcoreapp2.2",
            ".NETCoreApp,Version=v3.0" => "netcoreapp3.0",
            ".NETCoreApp,Version=v3.1" => "netcoreapp3.1",
            ".NETCoreApp,Version=v5.0" => "net5.0",
            ".NETCoreApp,Version=v6.0" => "net6.0",
            ".NETCoreApp,Version=v7.0" => "net7.0",
            ".NETCoreApp,Version=v8.0" => "net8.0",
            ".NETCoreApp,Version=v9.0" => "net9.0",
            _ when frameworkName.StartsWith(".NETFramework") =>
                frameworkName.Replace(".NETFramework,Version=v", string.Empty).Replace(".", string.Empty),
            _ when frameworkName.StartsWith(".NETCoreApp") =>
                frameworkName.Replace(".NETCoreApp,Version=v", string.Empty),
            _ => throw new NotSupportedException($"Unsupported framework: {frameworkName}")
        };
    }
}
