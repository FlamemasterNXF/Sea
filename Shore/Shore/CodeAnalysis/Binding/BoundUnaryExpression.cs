using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryOperator? Op { get; }
        public BoundExpression? Operand { get; }

        public BoundUnaryExpression(BoundUnaryOperator? op, BoundExpression? operand)
        {
            Op = op;
            Operand = operand;
        }

        public override TypeSymbol? Type => Op.ResultType;
        
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    }
}