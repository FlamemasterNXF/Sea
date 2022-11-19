namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class GlobalStatementNode : MemberNode
    {
        public StatementNode Statement { get; }
        public override TokType Type => TokType.GlobalStatement;

        public GlobalStatementNode(NodeTree nodeTree, StatementNode statement)
            : base (nodeTree)
        {
            Statement = statement;
        }
    }
}