namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class BreakStatementNode : StatementNode
    {
        public Token Keyword { get; }

        public BreakStatementNode(NodeTree nodeTree, Token keyword)
            : base(nodeTree)
        {
            Keyword = keyword;
        }

        public override TokType Type => TokType.BreakStatement;
    }
}