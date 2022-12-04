using System.Collections.Immutable;

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

        //Specialized Types (Internal Only)
        public static readonly TypeSymbol StringAndArray = new("stringAndArray");
        
        public static readonly TypeSymbol Bool = new ("bool");
        public static readonly TypeSymbol String = new ("string", StringAndArray);
        public static readonly TypeSymbol Number = new("number");
        public static readonly TypeSymbol Int64 = new ("int64", Number);
        public static readonly TypeSymbol Float64 = new ("float64", Number);
        public static readonly TypeSymbol Void = new("void");
        
        public static readonly TypeSymbol Array = new("[]");
        public static readonly TypeSymbol BoolArr = new ("bool[]", StringAndArray, Array);
        public static readonly TypeSymbol StringArr = new ("string[]", StringAndArray, Array);
        public static readonly TypeSymbol Int64Arr = new ("int64[]", StringAndArray, Array);
        public static readonly TypeSymbol Float64Arr = new ("float64[]", StringAndArray, Array);
        
        public static readonly TypeSymbol List = new("<>");
        public static readonly TypeSymbol BoolList = new ("bool<>", List);
        public static readonly TypeSymbol StringList = new ("string<>", List);
        public static readonly TypeSymbol Int64List = new ("int<>", List);
        public static readonly TypeSymbol Float64List = new ("float<>", List);

        public static bool CheckType(TypeSymbol actual, TypeSymbol required) =>
            actual == required || actual.ParentType == required;

        public static List<TypeSymbol>? GetChildrenTypes(TypeSymbol parent) =>
            parent == Number ? new List<TypeSymbol>() { Int64, Float64 } : null;

        public static TypeSymbol? GetAcceptedType(TypeSymbol arrType)
        {
            if (arrType == BoolArr || arrType == BoolList) return Bool;
            if (arrType == StringArr || arrType == StringList) return String;
            if (arrType == Int64Arr || arrType == Int64List) return Int64;
            if (arrType == Float64Arr || arrType == Float64List) return Float64;
            return null;
        }
    }
}