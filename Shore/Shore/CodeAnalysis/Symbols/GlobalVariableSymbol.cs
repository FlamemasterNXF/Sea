namespace Shore.CodeAnalysis.Symbols
{
    public class GlobalVariableSymbol : VariableSymbol
    {
        public override SymbolKind Kind => SymbolKind.GlobalVariable;

        internal GlobalVariableSymbol(string name, bool isReadOnly, TypeSymbol? type, bool isList = false)
            : base(name, isReadOnly, type, isList)
        {
        }
    }
}