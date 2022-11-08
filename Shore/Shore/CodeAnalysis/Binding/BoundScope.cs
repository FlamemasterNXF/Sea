using System.Collections.Immutable;
using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        public BoundScope? Parent { get; }
        private Dictionary<string, VariableSymbol>? _variables;
        private Dictionary<string, FunctionSymbol>? _functions;

        public BoundScope(BoundScope? parent)
        {
            Parent = parent;
        }

        public bool TryDeclareVariable(VariableSymbol variable)
        {
            _variables ??= new Dictionary<string, VariableSymbol>();
            if (_variables.ContainsKey(variable.Name)) return false;
            
            _variables.Add(variable.Name, variable);
            return true;
        }

        public bool TryLookupVariable(string name, out VariableSymbol? variable)
        {
            variable = null;

            if (_variables is not null && _variables.TryGetValue(name, out variable)) return true;

            if (Parent is null) return false;
            return Parent.TryLookupVariable(name, out variable);
        }

        public bool TryDeclareFunction(FunctionSymbol function)
        {
            _functions ??= new Dictionary<string, FunctionSymbol>();
            
            if (_functions.ContainsKey(function.Name)) return false;
            
            _functions.Add(function.Name, function);
            return true;
        }

        public bool TryLookupFunction(string name, out FunctionSymbol? function)
        {
            function = null;

            if (_functions is not null && _functions.TryGetValue(name, out function)) return true;

            if (Parent is null) return false;
            return Parent.TryLookupFunction(name, out function);
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
        {
            return _variables is null ? ImmutableArray<VariableSymbol>.Empty : _variables.Values.ToImmutableArray();
        }
        
        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
        {
            return _functions is null ? ImmutableArray<FunctionSymbol>.Empty : _functions.Values.ToImmutableArray();
        }
    }
}