namespace Shore.CodeAnalysis.Symbols
{
    public class VariableSymbol : Symbol
    {
        public TypeSymbol Type { get; }
        public bool IsReadOnly { get; }
        public override SymbolKind Kind => SymbolKind.Variable;

        internal VariableSymbol(string name, bool isReadOnly, TypeSymbol type) 
            : base(name)
        {
            IsReadOnly = isReadOnly;
            Type = type;
        }

    }
}