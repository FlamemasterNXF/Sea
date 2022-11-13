using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundExpression? Left { get; }
        public BoundBinaryOperator? Op { get; }
        public BoundExpression? Right { get; }

        public BoundBinaryExpression(BoundExpression? left, BoundBinaryOperator? op, BoundExpression? right)
        {
            Left = left;
            Op = op;
            Right = right;
        }

        public override TypeSymbol? Type => Op?.ResultType;

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    }
}