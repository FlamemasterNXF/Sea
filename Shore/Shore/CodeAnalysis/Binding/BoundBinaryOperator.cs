using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator
    {
        public TokType TokType { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public TypeSymbol LeftType { get; }
        public TypeSymbol RightType { get; }
        public TypeSymbol ResultType { get; }

        private BoundBinaryOperator(TokType tokType, BoundBinaryOperatorKind kind, TypeSymbol type)
            : this(tokType, kind, type, type, type){}
        
        private BoundBinaryOperator(TokType tokType, BoundBinaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType)
            : this(tokType, kind, operandType, operandType, resultType){}

        private BoundBinaryOperator(TokType tokType, BoundBinaryOperatorKind kind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
        {
            TokType = tokType;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            ResultType = resultType;
        }
        
        private static readonly List<BoundBinaryOperator> FixedOperators = new List<BoundBinaryOperator>()
        {
            new (TokType.CaratToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Int32),
            new (TokType.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Int32),
            new (TokType.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Int32),
            new (TokType.RightShiftToken, BoundBinaryOperatorKind.BitwiseRightShift, TypeSymbol.Int32),
            new (TokType.LeftShiftToken, BoundBinaryOperatorKind.BitwiseLeftShift, TypeSymbol.Int32),
            
            new (TokType.CaratToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Bool),
            new (TokType.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Bool),
            new (TokType.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Bool),
            new (TokType.DoubleAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, TypeSymbol.Bool),
            new (TokType.DoublePipeToken, BoundBinaryOperatorKind.LogicalOr, TypeSymbol.Bool),
            new (TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, TypeSymbol.Bool),
            new (TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, TypeSymbol.Bool),
        };

        private static BoundBinaryOperator[] Operators()
        {
            List<BoundBinaryOperator> dynamicOperators = new List<BoundBinaryOperator>();
            foreach (var type in TypeSymbol.GetChildrenTypes(TypeSymbol.Number)!)
            {
                dynamicOperators.Add(new BoundBinaryOperator(TokType.PlusToken, BoundBinaryOperatorKind.Addition, type));
                dynamicOperators.Add(new BoundBinaryOperator(TokType.DashToken, BoundBinaryOperatorKind.Subtraction, type));
                dynamicOperators.Add(new BoundBinaryOperator(TokType.StarToken, BoundBinaryOperatorKind.Multiplication, type));
                dynamicOperators.Add(new BoundBinaryOperator(TokType.SlashToken, BoundBinaryOperatorKind.Division, type));
                dynamicOperators.Add(new BoundBinaryOperator(TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, type, TypeSymbol.Bool));
                dynamicOperators.Add(new BoundBinaryOperator(TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, type, TypeSymbol.Bool));
                dynamicOperators.Add(new BoundBinaryOperator(TokType.GreaterThanToken, BoundBinaryOperatorKind.GreaterThan, type, TypeSymbol.Bool));
                dynamicOperators.Add(new BoundBinaryOperator(TokType.GreaterThanOrEqualToken, BoundBinaryOperatorKind.GreaterThanOrEqual, type, TypeSymbol.Bool));
                dynamicOperators.Add(new BoundBinaryOperator(TokType.LessThanToken, BoundBinaryOperatorKind.LessThan, type, TypeSymbol.Bool));
                dynamicOperators.Add(new BoundBinaryOperator(TokType.LessThanOrEqualToken, BoundBinaryOperatorKind.LessThanOrEqual, type, TypeSymbol.Bool));
            }
            
            var operators = dynamicOperators.Concat(FixedOperators);
            return operators.ToArray();
        }

        public static BoundBinaryOperator? Bind(TokType tokType, TypeSymbol leftType, TypeSymbol rightType)
        {
            foreach (var op in Operators())
            {
                if (op.TokType == tokType && op.LeftType == leftType && op.RightType == rightType) return op;
            }

            return null;
        }
    }
}