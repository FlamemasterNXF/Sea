using System.Collections.Immutable;

namespace Shore.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol Type { get; }
        public override SymbolKind Kind => SymbolKind.Function;

        public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type)
            : base(name)
        {
            Parameters = parameters;
            Type = type;
        }
    }
}