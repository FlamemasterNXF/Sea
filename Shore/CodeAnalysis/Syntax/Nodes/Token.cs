namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class Token : Node
    {
        public override TokType Type { get; }

        public override IEnumerable<Node> GetChildren()
        {
            return Enumerable.Empty<Node>();
        }

        public int Position { get; }
        public string? Text { get; }
        public object? Value { get; }
        public TextSpan Span => new(Position, Text.Length);

        public Token(TokType type, int position, string? text, object? value)
        {
            Type = type;
            Position = position;
            Text = text;
            Value = value;
        }
    }
}