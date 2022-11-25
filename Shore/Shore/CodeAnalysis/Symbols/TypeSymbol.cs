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
        public static readonly TypeSymbol Any = new ("any", null); //Internal usage ONLY.
        public static readonly TypeSymbol Bool = new ("bool", null);
        public static readonly TypeSymbol String = new ("string", null);
        public static readonly TypeSymbol Number = new("Number", null);
        public static readonly TypeSymbol Integer = new("integer", null);
        public static readonly TypeSymbol Float = new("float", null);
        public static readonly TypeSymbol Void = new("void", null);

        
        public static readonly TypeSymbol Int8 = new ("int8", Integer);
        public static readonly TypeSymbol Int16 = new ("int16", Integer);
        public static readonly TypeSymbol Int32 = new ("int32", Integer);
        public static readonly TypeSymbol Int64 = new ("int64", Integer);
        public static readonly TypeSymbol Float32 = new ("float32", Float);
        public static readonly TypeSymbol Float64 = new ("float64", Float);

        public static bool CheckType(TypeSymbol actual, TypeSymbol required) =>
            actual == required || actual.ParentType == required;

        public static List<TypeSymbol>? GetChildrenTypes(TypeSymbol parent)
        {
            if (parent == Bool) return null;
            if (parent == String) return null;
            if (parent == Integer) return new List<TypeSymbol>() { Int8, Int16, Int32, Int64 };
            if (parent == Float) return new List<TypeSymbol>() { Float32, Float64 };
            return parent == Number ? new List<TypeSymbol>() { Int8, Int16, Int32, Int64, Float32, Float64 } : null;
        }
    }
}