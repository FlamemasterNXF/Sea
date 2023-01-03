namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class DictDeclarationNode : StatementNode
    {
        public Token AType { get; }
        public Token Identifier { get; }
        public Token EqualsToken { get; }
        public Token OpenBraceToken { get; }
        public SeparatedNodeList<ExpressionNode> Keys { get; }
        public SeparatedNodeList<ExpressionNode> Values { get; }
        public Token CloseBraceToken { get; }
        
        public override TokType Type => TokType.DictDeclarationStatement;

        public DictDeclarationNode(NodeTree nodeTree, Token type, Token identifier, Token equalsToken,
            Token openBraceToken, SeparatedNodeList<ExpressionNode> keys, SeparatedNodeList<ExpressionNode> values,
            Token closeBraceToken)
            : base(nodeTree)

        {
            AType = type;
            Identifier = identifier;
            EqualsToken = equalsToken;
            OpenBraceToken = openBraceToken;
            Keys = keys;
            Values = values;
            CloseBraceToken = closeBraceToken;
        }
    }
    public sealed class ListDeclarationNode : StatementNode
    {
        public Token AType { get; }
        public Token Identifier { get; }
        public Token EqualsToken { get; }
        public Token OpenBraceToken { get; }
        public SeparatedNodeList<ExpressionNode> Members { get; }
        public Token CloseBraceToken { get; }
        
        public override TokType Type => TokType.ListDeclarationStatement;

        public ListDeclarationNode(NodeTree nodeTree, Token type, Token identifier, Token equalsToken,
            Token openBraceToken, SeparatedNodeList<ExpressionNode> members, Token closeBraceToken)
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