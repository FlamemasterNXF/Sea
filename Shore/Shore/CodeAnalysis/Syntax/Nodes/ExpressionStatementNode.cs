namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ExpressionStatementNode : StatementNode
    {
        public ExpressionNode Expression { get; }
        public override TokType Type => TokType.ExpressionStatement;

        public ExpressionStatementNode(ExpressionNode expression)
        {
            Expression = expression;
        }
    }
}