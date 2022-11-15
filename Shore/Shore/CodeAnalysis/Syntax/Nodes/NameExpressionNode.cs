namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class NameExpressionNode : ExpressionNode
    {
        public Token IdentifierToken { get; }

        public NameExpressionNode(NodeTree nodeTree, Token identifierToken)
            : base(nodeTree)
        {
            IdentifierToken = identifierToken;
        }

        public override TokType Type => TokType.NameExpression;
    }
}