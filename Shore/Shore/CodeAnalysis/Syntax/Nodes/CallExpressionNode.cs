namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class CallExpressionNode : ExpressionNode
    {
        public Token Identifier { get; }
        public Token OpenParenToken { get; }
        public SeparatedNodeList<ExpressionNode> Arguments { get; }
        public Token CloseParenToken { get; }
        public override TokType Type => TokType.CallExpression;

        public CallExpressionNode(Token identifier, Token openParenToken, SeparatedNodeList<ExpressionNode> arguments,
            Token closeParenToken)
        {
            Identifier = identifier;
            OpenParenToken = openParenToken;
            Arguments = arguments;
            CloseParenToken = closeParenToken;
        }
    }
}