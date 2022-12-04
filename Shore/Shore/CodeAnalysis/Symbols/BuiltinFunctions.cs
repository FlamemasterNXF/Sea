using System.Collections.Immutable;
using System.Reflection;

namespace Shore.CodeAnalysis.Symbols
{
    internal static class BuiltinFunctions
    {
        public static readonly FunctionSymbol Print = new("print",
            ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.Any)), TypeSymbol.Void);

        public static readonly FunctionSymbol Input = new("input", ImmutableArray<ParameterSymbol>.Empty,
            TypeSymbol.String);

        public static readonly FunctionSymbol Round = new("round",
            ImmutableArray.Create(new ParameterSymbol("value", TypeSymbol.Float64)), TypeSymbol.Int64);
        
        public static readonly FunctionSymbol Floor = new("floor",
            ImmutableArray.Create(new ParameterSymbol("value", TypeSymbol.Float64)), TypeSymbol.Int64);
        
        public static readonly FunctionSymbol Ceil = new("ceil",
            ImmutableArray.Create(new ParameterSymbol("value", TypeSymbol.Float64)), TypeSymbol.Int64);
        
        public static readonly FunctionSymbol Length = new("length",
            ImmutableArray.Create(new ParameterSymbol("array", TypeSymbol.Array)), TypeSymbol.Int64);
        
        internal static IEnumerable<FunctionSymbol?> GetAll() => typeof(BuiltinFunctions)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(FunctionSymbol))
            .Select(f => (FunctionSymbol?)f.GetValue(null));
    }
}