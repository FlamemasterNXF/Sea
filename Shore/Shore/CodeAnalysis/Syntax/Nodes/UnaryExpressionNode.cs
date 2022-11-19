namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class UnaryExpressionNode : ExpressionNode
    {
        public Token OperatorToken { get; }
        public ExpressionNode Operand { get; }

        public UnaryExpressionNode(NodeTree nodeTree, Token operatorToken, ExpressionNode operand)
            : base (nodeTree)
        {
            OperatorToken = operatorToken;
            Operand = operand;
        }

        public override TokType Type => TokType.UnaryExpression;
    }
}