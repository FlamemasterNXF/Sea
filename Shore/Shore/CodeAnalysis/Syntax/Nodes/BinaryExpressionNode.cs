namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class CompilationUnitNode : Node
    {
        public ExpressionNode Expression { get; }
        public Token EndOfFileToken { get; }
        public override TokType Type => TokType.CompilationUnit;

        public CompilationUnitNode(ExpressionNode expression, Token endOfFileToken)
        {
            Expression = expression;
            EndOfFileToken = endOfFileToken;
        }
    }
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