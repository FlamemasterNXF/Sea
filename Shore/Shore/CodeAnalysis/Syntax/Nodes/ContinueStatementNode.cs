namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ContinueStatementNode : StatementNode
    {
        public Token Keyword { get; }

        public ContinueStatementNode(Token keyword)
        {
            Keyword = keyword;
        }

        public override TokType Type => TokType.ContinueStatement;
    }
}