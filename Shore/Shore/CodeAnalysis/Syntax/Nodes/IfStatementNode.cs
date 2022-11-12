namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class IfStatementNode : StatementNode
    {
        public Token IfKeyword { get; }
        public ExpressionNode Condition { get; }
        public StatementNode? ThenStatement { get; }
        public ElseNode? ElseNode { get; }
        public override TokType Type => TokType.IfStatement;

        public IfStatementNode(Token ifKeyword, ExpressionNode condition, StatementNode? thenStatement,
            ElseNode? elseNode)
        {
            IfKeyword = ifKeyword;
            Condition = condition;
            ThenStatement = thenStatement;
            ElseNode = elseNode;
        }
    }
}