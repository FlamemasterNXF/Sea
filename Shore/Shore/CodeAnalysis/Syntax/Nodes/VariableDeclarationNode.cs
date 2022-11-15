namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class VariableDeclarationNode : StatementNode
    {
        public Token VType { get; }
        public Token Identifier { get; }
        public Token EqualsToken { get; }
        public ExpressionNode Initializer { get; }
        public override TokType Type => TokType.VariableDeclarationStatement;

        public VariableDeclarationNode(NodeTree nodeTree, Token type, Token identifier, Token equalsToken,
            ExpressionNode initializer)
            : base(nodeTree)
        {
            VType = type;
            Identifier = identifier;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }
    }
}