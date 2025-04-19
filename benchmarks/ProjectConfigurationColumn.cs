using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace AutoLoggerMessageGenerator.Benchmarks;

public class ProjectConfigurationColumn : IColumn
{
    public string Id => "ProjectConfiguration";
    
    public string ColumnName => "ProjectConfiguration";
    
    public bool AlwaysShow => true;
    
    public ColumnCategory Category => ColumnCategory.Job;

    public int PriorityInCategory => int.MaxValue;
        
    public bool IsNumeric => false;
    
    public UnitType UnitType => UnitType.Dimensionless;
    
    public string Legend => "The project configuration used to run the benchmark";

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase) => benchmarkCase.Descriptor.Type.Assembly.GetName().Name!;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
    
    public bool IsAvailable(Summary summary) => true;
    
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
}
