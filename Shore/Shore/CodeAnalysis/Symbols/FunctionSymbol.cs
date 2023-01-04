using System.Collections.Immutable;
using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public ImmutableArray<ParameterSymbol?> Parameters { get; }
        public TypeSymbol Type { get; }
        public bool IsExtension { get; }
        public FunctionDeclarationNode? Declaration { get; }
        public override SymbolKind Kind => SymbolKind.Function;

        public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type,
            bool isExtension = false, FunctionDeclarationNode? declaration = null)
            : base(name)

        {
            Parameters = parameters;
            Type = type;
            IsExtension = isExtension;
            Declaration = declaration;
        }
    }
}