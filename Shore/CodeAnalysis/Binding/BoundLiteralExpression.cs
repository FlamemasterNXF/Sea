namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public object Value { get; }

        public BoundLiteralExpression(object value)
        {
            Value = value;
        }

        public override Type Type => Value.GetType();

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    }
}