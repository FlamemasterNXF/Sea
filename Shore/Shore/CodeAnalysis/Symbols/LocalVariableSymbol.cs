namespace Shore.CodeAnalysis.Symbols
{
    public class LocalVariableSymbol : VariableSymbol
    {
        public override SymbolKind Kind => SymbolKind.LocalVariable;

        internal LocalVariableSymbol(string name, bool isReadOnly, TypeSymbol type, bool isList = false)
            : base(name, isReadOnly, type, isList)
        {
        }
    }
}