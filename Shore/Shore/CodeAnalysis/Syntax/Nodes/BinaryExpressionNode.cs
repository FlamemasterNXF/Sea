namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; }
        public Token OperatorToken { get; }
        public ExpressionNode Right { get; }

        public BinaryExpressionNode(ExpressionNode left, Token operatorToken, ExpressionNode right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }

        public override TokType Type => TokType.BinaryExpression;
    }
}