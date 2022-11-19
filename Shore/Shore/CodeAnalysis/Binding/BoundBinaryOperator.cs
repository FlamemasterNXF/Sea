using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax;
using static Shore.CodeAnalysis.Binding.BoundBinaryOperatorKind;
using static Shore.CodeAnalysis.Syntax.TokType;

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
            new (CaratToken, BitwiseXor, TypeSymbol.Int32),
            new (AmpersandToken, BitwiseAnd, TypeSymbol.Int32),
            new (PipeToken, BitwiseOr, TypeSymbol.Int32),
            new (RightShiftToken, BitwiseRightShift, TypeSymbol.Int32),
            new (LeftShiftToken, BitwiseLeftShift, TypeSymbol.Int32),
            
            new (CaratToken, BitwiseXor, TypeSymbol.Bool),
            new (AmpersandToken, BitwiseAnd, TypeSymbol.Bool),
            new (PipeToken, BitwiseOr, TypeSymbol.Bool),
            new (DoubleAmpersandToken, LogicalAnd, TypeSymbol.Bool),
            new (DoublePipeToken, LogicalOr, TypeSymbol.Bool),
            new (DoubleEqualsToken, LogicalEquals, TypeSymbol.Bool),
            new (BangEqualsToken, LogicalNotEquals, TypeSymbol.Bool),
            
            new (PlusToken, Addition, TypeSymbol.String),
            new (DoubleEqualsToken, LogicalEquals, TypeSymbol.String, TypeSymbol.Bool),
            new (BangEqualsToken, LogicalNotEquals, TypeSymbol.String, TypeSymbol.Bool)
        };

        private static BoundBinaryOperator[] Operators()
        {
            List<BoundBinaryOperator> dynamicOperators = new List<BoundBinaryOperator>();
            foreach (var type in TypeSymbol.GetChildrenTypes(TypeSymbol.Number)!)
            {
                dynamicOperators.Add(new BoundBinaryOperator(PlusToken, Addition, type));
                dynamicOperators.Add(new BoundBinaryOperator(DashToken, Subtraction, type));
                dynamicOperators.Add(new BoundBinaryOperator(StarToken, Multiplication, type));
                dynamicOperators.Add(new BoundBinaryOperator(SlashToken, Division, type));
                dynamicOperators.Add(new BoundBinaryOperator(DoubleEqualsToken, LogicalEquals, type, TypeSymbol.Bool));
                dynamicOperators.Add(new BoundBinaryOperator(BangEqualsToken, LogicalNotEquals, type, TypeSymbol.Bool));
                dynamicOperators.Add(new BoundBinaryOperator(GreaterThanToken, GreaterThan, type, TypeSymbol.Bool));
                dynamicOperators.Add(new BoundBinaryOperator(GreaterThanOrEqualToken, GreaterThanOrEqual, type, TypeSymbol.Bool));
                dynamicOperators.Add(new BoundBinaryOperator(LessThanToken, LessThan, type, TypeSymbol.Bool));
                dynamicOperators.Add(new BoundBinaryOperator(LessThanOrEqualToken, LessThanOrEqual, type, TypeSymbol.Bool));
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