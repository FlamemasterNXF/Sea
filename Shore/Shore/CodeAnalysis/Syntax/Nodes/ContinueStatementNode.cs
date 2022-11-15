namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ContinueStatementNode : StatementNode
    {
        public Token Keyword { get; }

        public ContinueStatementNode(NodeTree nodeTree, Token keyword)
            : base(nodeTree)
        {
            Keyword = keyword;
        }

        public override TokType Type => TokType.ContinueStatement;
    }
}