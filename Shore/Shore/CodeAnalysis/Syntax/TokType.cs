namespace Shore.CodeAnalysis.Syntax
{
    public enum TokType
    {
        UnknownToken,
        EndOfFileToken,
        WhitespaceToken,
        
        NumberToken,
        IdentifierToken,
        FalseKeyword,
        TrueKeyword,
        
        DashToken,
        StarToken,
        SlashToken,
        PlusToken,
        CloseParenToken,
        OpenParenToken,
        EqualsToken,
        
        BangToken,
        DoubleAmpersandToken,
        DoublePipeToken,
        DoubleEqualsToken,
        BangEqualsToken,

        LiteralExpression,
        BinaryExpression,
        ParenthesisExpression,
        UnaryExpression,
        NameExpression,
        AssignmentExpression,
        CompilationUnit,
        ExpressionStatement,
        BlockStatement,
        CloseBraceToken,
        OpenBraceToken,
        VariableDeclarationStatement,
        LetKeyword,
        ReadOnlyKeyword
    }
}