using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundArrayExpression : BoundExpression
    {
        public VariableSymbol? Array { get; }
        public BoundExpression Accessor { get; }

        public BoundArrayExpression(VariableSymbol? array, BoundExpression accessor)
        {
            Array = array;
            Accessor = accessor;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ArrayExpression;
        public override TypeSymbol Type => Array.Type;
    }
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public VariableSymbol? Variable { get; }

        public BoundVariableExpression(VariableSymbol? variable)
        {
            Variable = variable;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public override TypeSymbol Type => Variable.Type;
    }
}