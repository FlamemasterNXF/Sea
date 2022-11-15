namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ElseNode : Node
    {
        public Token ElseKeyword { get; }
        public StatementNode? ElseStatement { get; }
        public override TokType Type => TokType.ElseStatement;

        public ElseNode(NodeTree nodeTree, Token elseKeyword, StatementNode? elseStatement)
            : base(nodeTree)
        {
            ElseKeyword = elseKeyword;
            ElseStatement = elseStatement;
        }
    }
}