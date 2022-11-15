namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class WhileStatementNode : StatementNode
    {
        public Token WhileKeyword { get; }
        public ExpressionNode Condition { get; }
        public StatementNode? Body { get; }
        public override TokType Type => TokType.WhileStatement;

        public WhileStatementNode(NodeTree nodeTree, Token whileKeyword, ExpressionNode condition, StatementNode? body)
            : base(nodeTree)
        {
            WhileKeyword = whileKeyword;
            Condition = condition;
            Body = body;
        }
    }
}