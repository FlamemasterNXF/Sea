using System.Collections.Immutable;
using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        public BoundScope? Parent { get; }
        private Dictionary<string, Symbol>? _symbols;

        public BoundScope(BoundScope? parent)
        {
            Parent = parent;
        }

        public bool TryDeclareVariable(VariableSymbol variable) => TryDeclare(variable);
        
        public bool TryDeclareFunction(FunctionSymbol function) => TryDeclare(function);

        public bool TryLookupVariable(string name, out VariableSymbol? variable) => TryLookup(name, out variable);
        
        public bool TryLookupFunction(string name, out FunctionSymbol? function) => TryLookup(name, out function);
        
        public ImmutableArray<VariableSymbol> GetDeclaredVariables() => GetDeclared<VariableSymbol>();
        
        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions() => GetDeclared<FunctionSymbol>();

        private bool TryDeclare<TSymbol>(TSymbol symbol)
            where TSymbol : Symbol
        {
            _symbols ??= new Dictionary<string, Symbol>();
            if (_symbols.ContainsKey(symbol.Name)) return false;
            
            _symbols.Add(symbol.Name, symbol);
            return true;
        }
        
        private bool TryLookup<TSymbol>(string name, out TSymbol? symbol)
            where TSymbol : Symbol 
        {
            symbol = null;

            if (_symbols is not null && _symbols.TryGetValue(name, out var declaredSymbol))
            {
                if (declaredSymbol is TSymbol matchingSymbol)
                {
                    symbol = matchingSymbol;
                    return true;
                }

                return false;
            }

            return Parent is not null && Parent.TryLookup<TSymbol>(name, out symbol);
        }

        private ImmutableArray<TSymbol> GetDeclared<TSymbol>()
            where TSymbol : Symbol
        {
            if (_symbols is null) return ImmutableArray<TSymbol>.Empty;
            return _symbols.Values.OfType<TSymbol>().ToImmutableArray();
        }
    }
}