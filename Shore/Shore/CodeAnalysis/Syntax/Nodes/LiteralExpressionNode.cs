namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class LiteralExpressionNode : ExpressionNode
    {
        public Token LiteralToken { get; }
        public object? Value { get; }

        public LiteralExpressionNode(Token literalToken, object? value)
        {
            LiteralToken = literalToken;
            Value = value;
        }
        
        public LiteralExpressionNode(Token literalToken)
            : this(literalToken, literalToken.Value)
        {
        }

        public override TokType Type => TokType.LiteralExpression;
    }
}