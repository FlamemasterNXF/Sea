namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundWhileStatement : BoundLoopStatement
    {
        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;

        public BoundWhileStatement(BoundExpression condition, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
            : base(breakLabel, continueLabel)
        {
            Condition = condition;
            Body = body;
        }
    }
}