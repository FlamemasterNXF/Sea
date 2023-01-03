namespace Shore.CodeAnalysis.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        public TypeSymbol Type { get; }
        public int Length { get; }
        public bool IsList { get; }
        public bool IsDict { get; }
        public bool IsReadOnly { get; }

        //TODO: There may be a better way to implement length checks.
        
        internal VariableSymbol(string name, bool isReadOnly, TypeSymbol type, int length, bool isList, bool isDict) 
            : base(name)
        {
            IsReadOnly = isReadOnly;
            Type = type;
            Length = length;
            IsList = isList;
            IsDict = isDict;
        }

    }
}