namespace AutoLoggerMessageGenerator.UnitTests.Utilities;

public static class RoslynConfigurationUtilities
{
    public static string GetRoslynVersion()
    {
        #if Roslyn408
            return "Rosyln_4_08";
        #elif Roslyn411
            return "Roslyn_4_11";
        #elif Roslyn414
            return "Roslyn_4_14";
        #endif

        throw new ArgumentOutOfRangeException("Unsupported roslyn version");
    }
}
