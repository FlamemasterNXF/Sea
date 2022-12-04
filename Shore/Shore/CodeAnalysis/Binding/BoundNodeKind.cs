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
        ConditionalGotoStatement,
        GotoStatement,
        LabelStatement,
        NullExpression,
        CallExpression,
        ConversionExpression,
        ReturnStatement,
        ArrayDeclaration
    }
}