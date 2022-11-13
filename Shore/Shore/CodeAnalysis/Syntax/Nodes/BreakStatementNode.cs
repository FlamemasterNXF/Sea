namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class BreakStatementNode : StatementNode
    {
        public Token Keyword { get; }

        public BreakStatementNode(Token keyword)
        {
            Keyword = keyword;
        }

        public override TokType Type => TokType.BreakStatement;
    }
}