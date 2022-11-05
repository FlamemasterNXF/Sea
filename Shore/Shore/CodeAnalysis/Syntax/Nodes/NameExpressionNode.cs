namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class NameExpressionNode : ExpressionNode
    {
        public Token IdentifierToken { get; }

        public NameExpressionNode(Token identifierToken)
        {
            IdentifierToken = identifierToken;
        }

        public override TokType Type => TokType.NameExpression;
    }
}