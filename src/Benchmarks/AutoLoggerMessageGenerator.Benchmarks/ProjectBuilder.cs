using System.Diagnostics;

namespace AutoLoggerMessageGenerator.Benchmarks;

internal class ProjectBuilder(ProjectConfiguration projectConfiguration)
{
    public async Task<BuildResult> BuildAsync()
    {
        var baseDirectory = AppContext.BaseDirectory;

        var workingDirectory = Path.Join(baseDirectory, projectConfiguration.Name);

        CreateWorkingDirectory(workingDirectory);
        await GenerateEntryPoint(workingDirectory);
        CopyBenchmarkFiles(workingDirectory);

        var (outputFolder, projFilePath) = await GenerateProjectFile(projectConfiguration.Name, workingDirectory);

        await new CommandRunner(ProcessPriorityClass.Normal)
            .RunAsync("dotnet", $"build -c Release {projFilePath}");

        return new BuildResult
        {
            ProjectDirectory = workingDirectory,
            ExecutablePath = Path.Join(workingDirectory, outputFolder, $"{projectConfiguration.Name}.dll"),
        };
    }

    private static void CreateWorkingDirectory(string workingDirectory)
    {
        if (Directory.Exists(workingDirectory))
            Directory.Delete(workingDirectory, recursive: true);

        Directory.CreateDirectory(workingDirectory);
    }

    private static Task GenerateEntryPoint(string workingDirectory)
    {
        const string mainEntryPoint = """
                                      public class Program
                                      {
                                         public static void Main() {}
                                      }
                                      """;

        return File.WriteAllTextAsync(Path.Join(workingDirectory, "Program.cs"), mainEntryPoint);
    }

    private static void CopyBenchmarkFiles(string workingDirectory)
    {
        var benchmarkFiles = $"{AppContext.BaseDirectory}/BenchmarkFiles";
        foreach (string newPath in Directory.GetFiles(benchmarkFiles, "*.*",SearchOption.AllDirectories))
            File.Copy(newPath, newPath.Replace(benchmarkFiles, workingDirectory), true);
    }

    private async Task<(string OutputFolder, string projFilePath)> GenerateProjectFile(string projectName, string workingDirectory)
    {
        var telemetryConstant = projectConfiguration.References.Contains(PackagesProvider.MicrosoftExtensionsTelemetryPackage)
            ? "TELEMETRY"
            : string.Empty;

        var targetFramework = TargetFrameworkMonikerDetector.Detect();

        string outputFolder = Path.Join("bin", "output");
        const string interceptorNamespace = Constants.GeneratorNamespace;

        string projFileContent = $"""
                                  <Project Sdk="Microsoft.NET.Sdk">
                                    <PropertyGroup>
                                      <OutputType>Exe</OutputType>
                                      <TargetFramework>{targetFramework}</TargetFramework>
                                      <InterceptorsPreviewNamespaces>$(InterceptorsPreviewNamespaces);{interceptorNamespace}</InterceptorsPreviewNamespaces>
                                      <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
                                      <AppendRuntimeIdentifierToOutputPath>false</AppendRuntimeIdentifierToOutputPath>
                                      <IntermediateOutputPath>{outputFolder}</IntermediateOutputPath>
                                      <RootNamespace>{projectName}</RootNamespace>
                                      <DefineConstants>$(DefineConstants);{telemetryConstant}</DefineConstants>
                                    </PropertyGroup>

                                    <PropertyGroup>
                                         <PlatformTarget>AnyCPU</PlatformTarget>
                                         <DebugType>pdbonly</DebugType>
                                         <DebugSymbols>true</DebugSymbols>
                                         <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
                                         <Optimize>true</Optimize>
                                         <Configuration>Release</Configuration>
                                         <IsPackable>false</IsPackable>
                                     </PropertyGroup>

                                    <ItemGroup>
                                      {string.Join(Environment.NewLine, projectConfiguration.References)}
                                    </ItemGroup>
                                  </Project>
                                  """;

        var projFilePath = Path.Combine(workingDirectory, $"{projectName}.csproj");
        await File.WriteAllTextAsync(projFilePath, projFileContent);

        return (outputFolder, projFilePath);
    }

    internal class BuildResult
    {
        public required string ProjectDirectory { get; init; }
        public required string ExecutablePath { get; init; }
    }
}
