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

        public void WriteTo(TextWriter writer) => SymbolPrinter.WriteTo(this, writer);
        public override string ToString()
        {
            using var writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }
    }
}