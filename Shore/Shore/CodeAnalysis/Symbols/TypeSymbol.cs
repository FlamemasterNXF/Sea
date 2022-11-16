namespace Shore.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public TypeSymbol? ParentType { get; }
        public override SymbolKind Kind => SymbolKind.Type;
        
        private TypeSymbol(string name, TypeSymbol? parentType) : base(name)
        {
            ParentType = parentType;
        }
        
        public static readonly TypeSymbol Null = new("null", null);
        public static readonly TypeSymbol Any = new TypeSymbol("any", null); //Internal usage ONLY.
        public static readonly TypeSymbol Bool = new ("bool", null);
        public static readonly TypeSymbol String = new ("string", null);
        public static readonly TypeSymbol Number = new("Number", null);
        public static readonly TypeSymbol Void = new("void", null);

        
        public static readonly TypeSymbol Int8 = new ("int8", Number);
        public static readonly TypeSymbol Int16 = new ("int16", Number);
        public static readonly TypeSymbol Int32 = new ("int32", Number);
        public static readonly TypeSymbol Int64 = new ("int64", Number);

        public static bool CheckType(TypeSymbol actual, TypeSymbol required) =>
            actual == required || actual.ParentType == required;

        public static List<TypeSymbol?>? GetChildrenTypes(TypeSymbol parent)
        {
            if (parent == Bool) return null;
            if (parent == String) return null;
            return parent == Number ? new List<TypeSymbol?>() { Int8, Int16, Int32, Int64 } : null;
        }
    }
}