namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ArrayAccessExpressionNode : ExpressionNode
    {
        public Token OpenBracket { get; }
        public ExpressionNode Accessor { get; }
        public Token Identifier { get; }
        public Token CloseBracket { get; }

        public override TokType Type => TokType.ArrayAccessExpression;

        public ArrayAccessExpressionNode(NodeTree nodeTree, Token identifier, Token openBracket, ExpressionNode accessor, Token closeBracket)
            : base(nodeTree)
        {
            OpenBracket = openBracket;
            Accessor = accessor;
            Identifier = identifier;
            CloseBracket = closeBracket;
        }
    }
}