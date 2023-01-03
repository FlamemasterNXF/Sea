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
        public static readonly TypeSymbol BoolList = new ("bool<>", StringAndArray, List);
        public static readonly TypeSymbol StringList = new ("string<>", StringAndArray, List);
        public static readonly TypeSymbol Int64List = new ("int64<>", StringAndArray, List);
        public static readonly TypeSymbol Float64List = new ("float64<>", StringAndArray, List);
        
        public static readonly TypeSymbol Dictionary = new("{}");
        public static readonly TypeSymbol BoolDict = new ("bool{}", StringAndArray, Dictionary);
        public static readonly TypeSymbol StringDict = new ("string{}", StringAndArray, Dictionary);
        public static readonly TypeSymbol Int64Dict = new ("int64{}", StringAndArray, Dictionary);
        public static readonly TypeSymbol Float64Dict = new ("float64{}", StringAndArray, Dictionary);

        public static bool CheckType(TypeSymbol actual, TypeSymbol required) =>
            actual == required || actual.ParentType == required;

        public static List<TypeSymbol>? GetChildrenTypes(TypeSymbol parent) =>
            parent == Number ? new List<TypeSymbol>() { Int64, Float64 } : null;

        public static TypeSymbol? GetAcceptedType(TypeSymbol arrType)
        {
            if (arrType == BoolArr || arrType == BoolList || arrType == BoolDict) return Bool;
            if (arrType == StringArr || arrType == StringList || arrType == StringDict) return String;
            if (arrType == Int64Arr || arrType == Int64List || arrType == Int64Dict) return Int64;
            return arrType == Float64Arr || arrType == Float64List || arrType == Float64Dict ? Float64 : null;
        }
    }
}