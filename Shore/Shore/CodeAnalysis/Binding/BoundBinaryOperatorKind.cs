namespace Shore.CodeAnalysis.Binding
{
    internal enum BoundBinaryOperatorKind
    {
        Addition, 
        Subtraction,
        Multiplication,
        Division,
        Exponentiation,
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