using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace AutoLoggerMessageGenerator.Benchmarks;

internal class MultiBenchmarkRunner(ProjectConfiguration[] projectConfigurations, string[]? args = null)
{
    private static readonly ManualConfig RunConfiguration = ManualConfig.CreateMinimumViable()
        .WithOptions(ConfigOptions.JoinSummary)
        .WithOptions(ConfigOptions.DisableLogFile)
        .AddColumn(new ProjectConfigurationColumn())
        .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared))
        .AddExporter(MarkdownExporter.GitHub);

    public async Task Run()
    {
        var buildResults = await BuildProjects();
        var types = FindAllBenchmarks(buildResults);

        BenchmarkRunner.Run(types, RunConfiguration, args);

        CleanupGeneratedProjects(buildResults);
    }

    private async Task<LinkedList<ProjectBuilder.BuildResult>> BuildProjects()
    {
        if (!projectConfigurations.Any())
            throw new InvalidOperationException("No project configurations provided");

        var buildResults = new LinkedList<ProjectBuilder.BuildResult>();
        foreach (var projectConfiguration in projectConfigurations.AsParallel())
        {
            var buildResult = await new ProjectBuilder(projectConfiguration).BuildAsync().ConfigureAwait(false);
            buildResults.AddLast(buildResult);
        }

        return buildResults;
    }

    private static TypeInfo[] FindAllBenchmarks(LinkedList<ProjectBuilder.BuildResult> buildResults)
    {
        var types = buildResults.Select(b => Assembly.LoadFrom(b.ExecutablePath))
            .SelectMany(c => c.DefinedTypes)
            .Where(c => c.DeclaredMethods.Any(v => v.GetCustomAttribute(typeof(BenchmarkAttribute)) is not null))
            .ToArray();
        return types;
    }

    private static void CleanupGeneratedProjects(LinkedList<ProjectBuilder.BuildResult> buildResults)
    {
        foreach (var buildResult in buildResults)
            Directory.Delete(buildResult.ProjectDirectory, recursive: true);
    }
}
