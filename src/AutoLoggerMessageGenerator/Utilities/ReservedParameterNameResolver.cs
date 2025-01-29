namespace AutoLoggerMessageGenerator.Utilities;

internal static class ReservedParameterNameResolver
{
    private const char UniqueNameSuffix = '_';

    public static string GenerateUniqueIdentifierSuffix(string[] templateParametersNames)
    {
        if (templateParametersNames.All(name => !Constants.ReservedParameterNames.Contains(name)))
            return string.Empty;

        var length = templateParametersNames.Max(name => name.Length - name.TrimEnd(UniqueNameSuffix).Length + 1);
        return new(UniqueNameSuffix, length);
    }
}
