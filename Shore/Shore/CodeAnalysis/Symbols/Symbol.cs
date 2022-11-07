namespace Shore.CodeAnalysis.Symbols
{
    public abstract class Symbol
    {
        public string Name { get; }
        public abstract SymbolKind Kind { get; }

        private protected Symbol(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}