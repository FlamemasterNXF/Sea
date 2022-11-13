namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ReturnStatementNode : StatementNode
    {
        public Token Keyword { get; }
        public ExpressionNode? Expression { get; }

        public ReturnStatementNode(Token keyword, ExpressionNode? expression)
        {
            Keyword = keyword;
            Expression = expression;
        }

        public override TokType Type => TokType.ReturnStatement;
    }
}