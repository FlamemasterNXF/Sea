namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ParenthesisExpressionNode : ExpressionNode
    {
        public Token OpenParen { get; }
        public ExpressionNode Expression { get; }
        public Token CloseParen { get; }

        public ParenthesisExpressionNode(Token openParen, ExpressionNode expression, Token closeParen)
        {
            OpenParen = openParen;
            Expression = expression;
            CloseParen = closeParen;
        }

        public override TokType Type => TokType.ParenthesisExpression;
        public override IEnumerable<Node> GetChildren()
        {
            yield return OpenParen;
            yield return Expression;
            yield return CloseParen;
        }
    }
}