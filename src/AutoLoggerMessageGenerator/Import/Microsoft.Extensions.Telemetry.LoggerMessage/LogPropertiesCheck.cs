using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Gen.Logging.Parsing;
using Microsoft.Gen.Shared;

namespace AutoLoggerMessageGenerator.Import.Microsoft.Extensions.Telemetry.LoggerMessage;

// The original code is a private local function, so the simplest way is just to copy it from the source with adjustments
// source: https://github.dev/dotnet/extensions/blob/c08e5ac737d0830ff83c1c5ae8b084e6fce2b538/src/Generators/Microsoft.Gen.Logging/Parsing/Parser.LogProperties.cs#L334-L335
internal class LogPropertiesCheck
{
    private static readonly HashSet<TypeKind> _allowedTypeKinds = [TypeKind.Class, TypeKind.Struct, TypeKind.Interface];

    private readonly SymbolHolder? _symbolHolder;

    public LogPropertiesCheck(Compilation compilation) =>
        _symbolHolder = SymbolLoader.LoadSymbols(compilation, static (_,_,_) => { });

    public bool IsApplicable(ITypeSymbol symType)
    {
        // TODO: Debug why sometimes its null?
        if (_symbolHolder is null) return false;

        var isRegularType = symType.Kind == SymbolKind.NamedType &&
            _allowedTypeKinds.Contains(symType.TypeKind) &&
            !symType.IsStatic;

        if (symType.IsNullableOfT())
            symType = ((INamedTypeSymbol)symType).TypeArguments[0];

        return isRegularType && !symType.IsSpecialType(_symbolHolder);
    }
}
