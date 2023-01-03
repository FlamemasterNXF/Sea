namespace Shore.CodeAnalysis.Symbols
{
    public class LocalVariableSymbol : VariableSymbol
    {
        public override SymbolKind Kind => SymbolKind.LocalVariable;

        internal LocalVariableSymbol(string name, bool isReadOnly, TypeSymbol type, int length = 0, bool isList = false,
            bool isDict = false)
            : base(name, isReadOnly, type, length, isList, isDict)

        {
        }
    }
}