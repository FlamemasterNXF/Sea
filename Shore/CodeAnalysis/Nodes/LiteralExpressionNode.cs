using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis.Nodes
{
    public sealed class NumberExpressionNode : ExpressionNode
    {
        public Token LiteralToken { get; }

        public NumberExpressionNode(Token literalToken)
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