namespace Shore.CodeAnalysis.Symbols
{
    public class LocalVariableSymbol : VariableSymbol
    {
        public override SymbolKind Kind => SymbolKind.LocalVariable;

        internal LocalVariableSymbol(string name, bool isReadOnly, TypeSymbol type)
            : base(name, isReadOnly, type)
        {
        }
    }
}