namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundExpression Condition { get; }
        public BoundStatement ThenStatement { get; }
        public BoundStatement? ElseStatement { get; }
        public override BoundNodeKind Kind => BoundNodeKind.IfStatement;

        public BoundIfStatement(BoundExpression condition, BoundStatement thenStatement, BoundStatement? elseStatement)
        {
            Condition = condition;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;
        }
    }
}