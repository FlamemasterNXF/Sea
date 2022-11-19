namespace Shore.CodeAnalysis.Binding
{
    internal enum BoundBinaryOperatorKind
    {
        Addition, 
        Subtraction,
        Multiplication,
        Division,
        LogicalOr,
        LogicalAnd,
        LogicalNotEquals,
        LogicalEquals,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        BitwiseXor,
        BitwiseAnd,
        BitwiseOr,
        BitwiseRightShift,
        BitwiseLeftShift
    }
}