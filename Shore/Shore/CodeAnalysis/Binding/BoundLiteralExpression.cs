using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public object Value { get; }

        public BoundLiteralExpression(object value)
        {
            Value = value;

            Type = value switch
            {
                bool => TypeSymbol.Bool,
                string => TypeSymbol.String,
                byte => TypeSymbol.Int8,
                short => TypeSymbol.Int16,
                int => TypeSymbol.Int32,
                long => TypeSymbol.Int64,
                _ => throw new Exception($"Unexpected Literal '{value}' of Type {value.GetType()}")
            };
        }

        public override TypeSymbol Type { get;  }

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    }
}