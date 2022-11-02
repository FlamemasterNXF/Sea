using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis.Nodes
{
    public sealed class LiteralExpressionNode : ExpressionNode
    {
        public Token LiteralToken { get; }

        public LiteralExpressionNode(Token literalToken)
        {
            LiteralToken = literalToken;
        }

        public override TokType Type => TokType.LiteralExpression;
        public override IEnumerable<Node> GetChildren()
        {
            yield return LiteralToken;
        }
    }
}