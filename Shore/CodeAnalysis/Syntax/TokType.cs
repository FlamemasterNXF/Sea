namespace Shore.CodeAnalysis.Syntax
{
    public enum TokType
    {
        NumberToken,
        WhitespaceToken,
        DashToken,
        StarToken,
        SlashToken,
        PlusToken,
        CloseParenToken,
        OpenParenToken,
        UnknownToken,
        EndOfFileToken,
        NumberExpression,
        BinaryExpression,
        ParenthesisExpression
    }
}