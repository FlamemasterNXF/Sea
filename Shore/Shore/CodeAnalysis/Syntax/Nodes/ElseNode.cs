namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ElseNode : Node
    {
        public Token ElseKeyword { get; }
        public StatementNode? ElseStatement { get; }
        public override TokType Type => TokType.ElseStatement;

        public ElseNode(Token elseKeyword, StatementNode? elseStatement)
        {
            ElseKeyword = elseKeyword;
            ElseStatement = elseStatement;
        }
    }
}