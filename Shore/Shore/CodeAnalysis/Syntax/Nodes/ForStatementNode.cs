namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ForStatementNode : StatementNode
    {
        public Token Keyword { get; }
        public Token Identifier { get; }
        public Token EqualsToken { get; }
        public ExpressionNode LowerBound { get; }
        public Token UntilToken { get; }
        public ExpressionNode UpperBound { get; }
        public StatementNode Body { get; }
        public override TokType Type => TokType.ForStatement;

        public ForStatementNode(Token keyword, Token identifier, Token equalsToken, ExpressionNode lowerBound,
            Token untilToken, ExpressionNode upperBound, StatementNode body)
        {
            Keyword = keyword;
            Identifier = identifier;
            EqualsToken = equalsToken;
            LowerBound = lowerBound;
            UntilToken = untilToken;
            UpperBound = upperBound;
            Body = body;
        }
    }
}