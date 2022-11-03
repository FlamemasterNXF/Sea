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
            //INTEGER TYPES
            new BoundBinaryOperator(TokType.PlusToken, BoundBinaryOperatorKind.Addition, typeof(int)),
            new BoundBinaryOperator(TokType.DashToken, BoundBinaryOperatorKind.Subtraction, typeof(int)),
            new BoundBinaryOperator(TokType.StarToken, BoundBinaryOperatorKind.Multiplication, typeof(int)),
            new BoundBinaryOperator(TokType.SlashToken, BoundBinaryOperatorKind.Division, typeof(int)),
            new BoundBinaryOperator(TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, typeof(int), typeof(bool)),
            new BoundBinaryOperator(TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, typeof(int), typeof(bool)),
            
            //BOOLEAN TYPES
            new BoundBinaryOperator(TokType.DoubleAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, typeof(bool)),
            new BoundBinaryOperator(TokType.DoublePipeToken, BoundBinaryOperatorKind.LogicalOr, typeof(bool)),
            new BoundBinaryOperator(TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, typeof(bool)),
            new BoundBinaryOperator(TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, typeof(bool)),
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