using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis.Nodes
{
    public sealed class UnaryExpressionNode : ExpressionNode
    {
        public Token OperatorToken { get; }
        public ExpressionNode Operand { get; }

        public UnaryExpressionNode(Token operatorToken, ExpressionNode operand)
        {
            OperatorToken = operatorToken;
            Operand = operand;
        }

        public override TokType Type => TokType.UnaryExpression;
        public override IEnumerable<Node> GetChildren()
        {
            yield return OperatorToken;
            yield return Operand;
        }
    }
}