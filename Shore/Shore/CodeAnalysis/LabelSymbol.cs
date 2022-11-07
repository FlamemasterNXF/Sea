namespace Shore.CodeAnalysis.Binding
{
    internal sealed class LabelSymbol
    {
        public string Name { get; }

        internal LabelSymbol(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
    }
}