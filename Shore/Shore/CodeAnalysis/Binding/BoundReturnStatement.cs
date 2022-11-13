namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundReturnStatement : BoundStatement
    {
        public BoundExpression? Expression { get; }

        public BoundReturnStatement(BoundExpression? expression)
        {
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;
    }
}