using System.Collections.Generic;
using System.Linq;
using AutoLoggerMessageGenerator.Emitters;

namespace AutoLoggerMessageGenerator.Utilities;

internal class ParameterNameNormalizer
{
    private readonly Dictionary<string, int> _nameConflictsCounter = 
        LoggerExtensionsEmitter.ReservedArgumentNames.ToDictionary(c => c, v => 1);
    
    public string Normalize(string parameterName) =>
        LoggerExtensionsEmitter.ReservedArgumentNames.Contains(parameterName) 
            ? $"{parameterName}{_nameConflictsCounter[parameterName]++}" 
            : parameterName;
}
