using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator
    {
        public TokType TokType { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public Type LeftType { get; }
        public Type RightType { get; }
        public Type ResultType { get; }

        private BoundBinaryOperator(TokType tokType, BoundBinaryOperatorKind kind, Type type)
            : this(tokType, kind, type, type, type){}
        
        private BoundBinaryOperator(TokType tokType, BoundBinaryOperatorKind kind, Type operandType, Type resultType)
            : this(tokType, kind, operandType, operandType, resultType){}

        private BoundBinaryOperator(TokType tokType, BoundBinaryOperatorKind kind, Type leftType, Type rightType, Type resultType)
        {
            TokType = tokType;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }
        
        private static readonly BoundBinaryOperator[] Operators =
        {
            new (TokType.PlusToken, BoundBinaryOperatorKind.Addition, typeof(int)),
            new (TokType.DashToken, BoundBinaryOperatorKind.Subtraction, typeof(int)),
            new (TokType.StarToken, BoundBinaryOperatorKind.Multiplication, typeof(int)),
            new (TokType.SlashToken, BoundBinaryOperatorKind.Division, typeof(int)),
            new (TokType.CaratToken, BoundBinaryOperatorKind.BitwiseXor, typeof(int)),
            new (TokType.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, typeof(int)),
            new (TokType.PipeToken, BoundBinaryOperatorKind.BitwiseOr, typeof(int)),
            new (TokType.RightShiftToken, BoundBinaryOperatorKind.BitwiseRightShift, typeof(int)),
            new (TokType.LeftShiftToken, BoundBinaryOperatorKind.BitwiseLeftShift, typeof(int)),
            new (TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, typeof(int), typeof(bool)),
            new (TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, typeof(int), typeof(bool)),
            new (TokType.GreaterThanToken, BoundBinaryOperatorKind.GreaterThan, typeof(int), typeof(bool)),
            new (TokType.GreaterThanOrEqualToken, BoundBinaryOperatorKind.GreaterThanOrEqual, typeof(int), typeof(bool)),
            new (TokType.LessThanToken, BoundBinaryOperatorKind.LessThan, typeof(int), typeof(bool)),
            new (TokType.LessThanOrEqualToken, BoundBinaryOperatorKind.LessThanOrEqual, typeof(int), typeof(bool)),
            
            new (TokType.CaratToken, BoundBinaryOperatorKind.BitwiseXor, typeof(bool)),
            new (TokType.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, typeof(bool)),
            new (TokType.PipeToken, BoundBinaryOperatorKind.BitwiseOr, typeof(bool)),
            new (TokType.DoubleAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, typeof(bool)),
            new (TokType.DoublePipeToken, BoundBinaryOperatorKind.LogicalOr, typeof(bool)),
            new (TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, typeof(bool)),
            new (TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, typeof(bool)),
        };

        public static BoundBinaryOperator? Bind(TokType tokType, Type leftType, Type rightType)
        {
            foreach (var op in Operators)
            {
                if (op.TokType == tokType && op.LeftType == leftType && op.RightType == rightType) return op;
            }

            return null;
        }
    }
}