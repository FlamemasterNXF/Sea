using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis.Nodes
{
    public sealed class NumberExpressionNode : ExpressionNode
    {
        public Token NumberToken { get; }

        public NumberExpressionNode(Token numberToken)
        {
            NumberToken = numberToken;
        }

        public override TokType Type => TokType.NumberExpression;
        public override IEnumerable<Node> GetChildren()
        {
            yield return NumberToken;
        }
    }
}