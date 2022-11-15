namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ExpressionStatementNode : StatementNode
    {
        public ExpressionNode Expression { get; }
        public override TokType Type => TokType.ExpressionStatement;

        public ExpressionStatementNode(NodeTree nodeTree, ExpressionNode expression)
            : base(nodeTree)
        {
            Expression = expression;
        }
    }
}