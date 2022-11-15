namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; }
        public Token OperatorToken { get; }
        public ExpressionNode Right { get; }

        public BinaryExpressionNode(NodeTree nodeTree, ExpressionNode left, Token operatorToken, ExpressionNode right)
            : base(nodeTree)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }

        public override TokType Type => TokType.BinaryExpression;
    }
}