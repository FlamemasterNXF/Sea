namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ParenthesisExpressionNode : ExpressionNode
    {
        public Token OpenParen { get; }
        public ExpressionNode Expression { get; }
        public Token CloseParen { get; }

        public ParenthesisExpressionNode(NodeTree nodeTree, Token openParen, ExpressionNode expression, Token closeParen)
            : base(nodeTree)
        {
            OpenParen = openParen;
            Expression = expression;
            CloseParen = closeParen;
        }

        public override TokType Type => TokType.ParenthesisExpression;
    }
}