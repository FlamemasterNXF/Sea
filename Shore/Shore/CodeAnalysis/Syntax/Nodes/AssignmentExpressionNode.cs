namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class AssignmentExpressionNode : ExpressionNode
    {
        public Token IdentifierToken { get; }
        public Token EqualsToken { get; }
        public ExpressionNode Expression { get; }

        public AssignmentExpressionNode(Token identifierToken, Token equalsToken, ExpressionNode expression)
        {
            IdentifierToken = identifierToken;
            EqualsToken = equalsToken;
            Expression = expression;
        }

        public override TokType Type => TokType.AssignmentExpression;
    }
}