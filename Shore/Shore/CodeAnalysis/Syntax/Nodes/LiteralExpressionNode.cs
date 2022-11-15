namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class LiteralExpressionNode : ExpressionNode
    {
        public Token LiteralToken { get; }
        public object? Value { get; }

        public LiteralExpressionNode(NodeTree nodeTree, Token literalToken)
            : this(nodeTree, literalToken, literalToken.Value)
        {
        }
        
        public LiteralExpressionNode(NodeTree nodeTree, Token literalToken, object? value)
            : base(nodeTree)
        {
            LiteralToken = literalToken;
            Value = value;
        }

        public override TokType Type => TokType.LiteralExpression;
    }
}