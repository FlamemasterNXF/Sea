namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ExtensionCallExpressionNode : ExpressionNode
    {
        public Token CType { get; }
        public Token DotToken { get; }
        public Token Identifier { get; }
        public Token OpenParenToken { get; }
        public SeparatedNodeList<ExpressionNode> Arguments { get; }
        public Token CloseParenToken { get; }
        public override TokType Type => TokType.ExtensionCallExpression;

        public ExtensionCallExpressionNode(NodeTree nodeTree, Token cType, Token dotToken, Token identifier, Token openParenToken,
            SeparatedNodeList<ExpressionNode> arguments, Token closeParenToken)
            : base(nodeTree)
        {
            CType = cType;
            DotToken = dotToken;
            Identifier = identifier;
            OpenParenToken = openParenToken;
            Arguments = arguments;
            CloseParenToken = closeParenToken;
        }
    }
}