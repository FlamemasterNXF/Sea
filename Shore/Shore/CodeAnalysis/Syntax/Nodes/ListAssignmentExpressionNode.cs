namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ListAssignmentExpressionNode : ExpressionNode
    {
        public Token IdentifierToken { get; }
        public Token EqualsToken { get; }
        public ExpressionNode Expression { get; }
        public ExpressionNode Accessor { get; }

        public ListAssignmentExpressionNode(NodeTree nodeTree, Token identifierToken, Token equalsToken,
            ExpressionNode expression, ExpressionNode accessor)
            : base(nodeTree)
        {
            IdentifierToken = identifierToken;
            EqualsToken = equalsToken;
            Expression = expression;
            Accessor = accessor;
        }

        public override TokType Type => TokType.ListAssignmentExpression;
    }
}