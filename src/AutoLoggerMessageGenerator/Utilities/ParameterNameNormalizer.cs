using System.Collections.Generic;
using System.Linq;

namespace AutoLoggerMessageGenerator.Utilities;

internal class ParameterNameNormalizer
{
    private readonly Dictionary<string, int> _nameConflictsCounter = 
        Constants.ReservedArgumentNames.ToDictionary(c => c, v => 1);
    
    public string Normalize(string parameterName)
    {
        parameterName = parameterName.StartsWith("@") ? parameterName : '@' + parameterName;
        return Constants.ReservedArgumentNames.Contains(parameterName)
            ? $"{parameterName}{_nameConflictsCounter[parameterName]++}"
            : parameterName;
    }
}
