namespace Shore.CodeAnalysis.Symbols
{
    public class GlobalVariableSymbol : VariableSymbol
    {
        public override SymbolKind Kind => SymbolKind.GlobalVariable;

        internal GlobalVariableSymbol(string name, bool isReadOnly, TypeSymbol? type, int length = 0,
            bool isList = false, bool isDict = false)
            : base(name, isReadOnly, type, length, isList, isDict)

        {
        }
    }
}