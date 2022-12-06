namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ListDeclarationNode : StatementNode
    {
        public Token AType { get; }
        public Token Identifier { get; }
        public Token EqualsToken { get; }
        public Token OpenBraceToken { get; }
        public SeparatedNodeList<LiteralExpressionNode> Members { get; }
        public Token CloseBraceToken { get; }
        
        public override TokType Type => TokType.ListDeclarationStatement;

        public ListDeclarationNode(NodeTree nodeTree, Token type, Token identifier, Token equalsToken,
            Token openBraceToken, SeparatedNodeList<LiteralExpressionNode> members, Token closeBraceToken)
            : base(nodeTree)
        {
            AType = type;
            Identifier = identifier;
            EqualsToken = equalsToken;
            OpenBraceToken = openBraceToken;
            Members = members;
            CloseBraceToken = closeBraceToken;
        }
    }
}