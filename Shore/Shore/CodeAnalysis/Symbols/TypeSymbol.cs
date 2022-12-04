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

        
        //public static readonly TypeSymbol Int8 = new ("int8", Integer);
        //public static readonly TypeSymbol Int16 = new ("int16", Integer);
        //public static readonly TypeSymbol Int32 = new ("int32", Integer);
        public static readonly TypeSymbol Int64 = new ("int64", Integer);
        //public static readonly TypeSymbol Float32 = new ("float32", Float);
        public static readonly TypeSymbol Float64 = new ("float64", Float);

        public static readonly TypeSymbol Array = new("[]", null);
        public static readonly TypeSymbol BoolArr = new ("bool", Array);
        public static readonly TypeSymbol StringArr = new ("string[]", Array);
        public static readonly TypeSymbol NumberArr = new("number[]", Array);
        public static readonly TypeSymbol Int64Arr = new ("int64", NumberArr);
        public static readonly TypeSymbol Float64Arr = new ("float64", NumberArr);

        public static bool CheckType(TypeSymbol actual, TypeSymbol required) =>
            actual == required || actual.ParentType == required;

        public static List<TypeSymbol>? GetChildrenTypes(TypeSymbol parent)
        {
            if (parent == NumberArr) return new List<TypeSymbol>() { Int64Arr, Float64Arr };
            if (parent == Integer) return new List<TypeSymbol>() { Int64 };
            if (parent == Float) return new List<TypeSymbol>() { Float64 };
            return parent == Number ? new List<TypeSymbol>() { Int64, Float64 } : null;
        }

        public static TypeSymbol? GetAcceptedType(TypeSymbol arrType)
        {
            if (arrType == BoolArr) return Bool;
            if (arrType == StringArr) return String;
            if (arrType == Int64Arr) return Int64;
            return arrType == Float64Arr ? Float64 : null;
        }
    }
}