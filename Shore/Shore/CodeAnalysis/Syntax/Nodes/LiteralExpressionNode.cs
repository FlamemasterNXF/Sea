namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class LiteralExpressionNode : ExpressionNode
    {
        public Token LiteralToken { get; }
        public object? Value { get; }
        public bool IsFloat { get; }

        public LiteralExpressionNode(NodeTree nodeTree, Token literalToken, bool isFloat = false)
            : this(nodeTree, literalToken, literalToken.Value, isFloat)
        {
        }

        public LiteralExpressionNode(NodeTree nodeTree, Token literalToken, object? value, bool isFloat)
            : base(nodeTree)
        {
            LiteralToken = literalToken;
            Value = value;
            IsFloat = isFloat;
        }

        public override TokType Type => TokType.LiteralExpression;
    }
}