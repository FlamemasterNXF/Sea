namespace Shore.CodeAnalysis
{
    public sealed class VariableSymbol
    {
        public string Name { get; }
        public Type Type { get; }

        internal VariableSymbol(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}