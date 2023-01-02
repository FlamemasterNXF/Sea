namespace Shore.CodeAnalysis.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        public TypeSymbol Type { get; }
        public bool IsList { get; }
        public bool IsReadOnly { get; }

        internal VariableSymbol(string name, bool isReadOnly, TypeSymbol type, bool isList) 
            : base(name)
        {
            IsReadOnly = isReadOnly;
            Type = type;
            IsList = isList;
        }

    }
}