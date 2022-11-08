namespace Shore.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        UnaryExpression,
        LiteralExpression,
        BinaryExpression,
        VariableExpression,
        AssignmentExpression,
        ExpressionStatement,
        BlockStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        ForStatement,
        ConiditonalGotoStatement,
        GotoStatement,
        LabelStatement,
        NullExpression,
        CallExpression
    }
}