namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundConditionalGotoStatement : BoundStatement
    {
        public BoundConditionalGotoStatement(LabelSymbol label, BoundExpression condition, bool jumpIfTrue = true)
        {
            Label = label;
            Condition = condition;
            JumpIfTrue = jumpIfTrue;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConiditonalGotoStatement;
        public LabelSymbol Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfTrue { get; }
    }
}