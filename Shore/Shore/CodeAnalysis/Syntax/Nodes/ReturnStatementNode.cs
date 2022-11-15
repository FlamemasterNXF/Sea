namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ReturnStatementNode : StatementNode
    {
        public Token Keyword { get; }
        public ExpressionNode? Expression { get; }

        public ReturnStatementNode(NodeTree nodeTree, Token keyword, ExpressionNode? expression)
            : base(nodeTree)
        {
            Keyword = keyword;
            Expression = expression;
        }

        public override TokType Type => TokType.ReturnStatement;
    }
}