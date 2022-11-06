namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class VariableDeclarationNode : StatementNode
    {
        public Token Keyword { get; }
        public Token Identifier { get; }
        public Token EqualsToken { get; }
        public ExpressionNode Initializer { get; }
        public override TokType Type => TokType.VariableDeclarationStatement;

        public VariableDeclarationNode(Token keyword, Token identifier, Token equalsToken, ExpressionNode initializer)
        {
            Keyword = keyword;
            Identifier = identifier;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }
    }
}