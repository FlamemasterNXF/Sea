namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryOperatorKind OperatorKind { get; }
        public BoundExpression Operand { get; }

        public BoundUnaryExpression(BoundUnaryOperatorKind operatorKind, BoundExpression operand)
        {
            OperatorKind = operatorKind;
            Operand = operand;
        }

        public override Type Type => Operand.Type;
        
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    }
}