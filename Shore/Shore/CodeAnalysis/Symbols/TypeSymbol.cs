namespace Shore.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public TypeSymbol? ParentType { get; }
        public TypeSymbol HeadType { get; }
        public override SymbolKind Kind => SymbolKind.Type;

        private TypeSymbol(string name) : this(name, Any, Any){}
        private TypeSymbol(string name, TypeSymbol parentType) : this(name, parentType, Any){}
        private TypeSymbol(string name, TypeSymbol? parentType, TypeSymbol headType) : base(name)
        {
            ParentType = parentType;
            HeadType = headType;
        }
        
        public static readonly TypeSymbol Null = new("null", null, null);
        public static readonly TypeSymbol Any = new ("any", null, null); //Internal usage ONLY.
        public static readonly TypeSymbol Bool = new ("bool");
        public static readonly TypeSymbol String = new ("string");
        public static readonly TypeSymbol Number = new("number");
        public static readonly TypeSymbol Int64 = new ("int64", Number);
        public static readonly TypeSymbol Float64 = new ("float64", Number);
        public static readonly TypeSymbol Void = new("void");
        
        public static readonly TypeSymbol Array = new("[]");
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