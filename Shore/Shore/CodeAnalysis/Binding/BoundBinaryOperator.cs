using System.Collections.Immutable;
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

        private static readonly List<BoundBinaryOperator> Operators = new()
        {
            new BoundBinaryOperator(CaratToken, BitwiseXor, TypeSymbol.Int64),
            new BoundBinaryOperator(AmpersandToken, BitwiseAnd, TypeSymbol.Int64),
            new BoundBinaryOperator(PipeToken, BitwiseOr, TypeSymbol.Int64),
            new BoundBinaryOperator(RightShiftToken, BitwiseRightShift, TypeSymbol.Int64),
            new BoundBinaryOperator(LeftShiftToken, BitwiseLeftShift, TypeSymbol.Int64),
            
            new BoundBinaryOperator(CaratToken, BitwiseXor, TypeSymbol.Bool),
            new BoundBinaryOperator(AmpersandToken, BitwiseAnd, TypeSymbol.Bool),
            new BoundBinaryOperator(PipeToken, BitwiseOr, TypeSymbol.Bool),
            new BoundBinaryOperator(DoubleAmpersandToken, BitwiseAnd, TypeSymbol.Bool),
            new BoundBinaryOperator(DoublePipeToken, BitwiseOr, TypeSymbol.Bool),
            new BoundBinaryOperator(DoubleEqualsToken, LogicalEquals, TypeSymbol.Bool),
            new BoundBinaryOperator(BangEqualsToken, LogicalNotEquals, TypeSymbol.Bool),
            
            new BoundBinaryOperator(PlusToken, Addition, TypeSymbol.String),
            new BoundBinaryOperator(SlashToken, Division, TypeSymbol.String, TypeSymbol.Int64, TypeSymbol.String),
            new BoundBinaryOperator(DoubleEqualsToken, LogicalEquals, TypeSymbol.String, TypeSymbol.Bool),
            new BoundBinaryOperator(BangEqualsToken, LogicalNotEquals, TypeSymbol.String, TypeSymbol.Bool),
            
            new BoundBinaryOperator(PlusToken, Addition, TypeSymbol.Int64),
            new BoundBinaryOperator(DashToken, Subtraction, TypeSymbol.Int64),
            new BoundBinaryOperator(StarToken, Multiplication, TypeSymbol.Int64),
            new BoundBinaryOperator(SlashToken, Division, TypeSymbol.Int64),
            new BoundBinaryOperator(DoubleStarToken, Exponentiation, TypeSymbol.Int64),
            new BoundBinaryOperator(DoubleEqualsToken, LogicalEquals, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(BangEqualsToken, LogicalNotEquals, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(GreaterThanToken, GreaterThan, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(GreaterThanOrEqualToken, GreaterThanOrEqual, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(LessThanToken, LessThan, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(LessThanOrEqualToken, LessThanOrEqual, TypeSymbol.Int64, TypeSymbol.Bool),
            
            new BoundBinaryOperator(PlusToken, Addition, TypeSymbol.Float64),
            new BoundBinaryOperator(DashToken, Subtraction, TypeSymbol.Float64),
            new BoundBinaryOperator(StarToken, Multiplication, TypeSymbol.Float64),
            new BoundBinaryOperator(SlashToken, Division, TypeSymbol.Float64),
            new BoundBinaryOperator(DoubleStarToken, Exponentiation, TypeSymbol.Float64),
            new BoundBinaryOperator(DoubleEqualsToken, LogicalEquals, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(BangEqualsToken, LogicalNotEquals, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(GreaterThanToken, GreaterThan, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(GreaterThanOrEqualToken, GreaterThanOrEqual, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(LessThanToken, LessThan, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(LessThanOrEqualToken, LessThanOrEqual, TypeSymbol.Float64, TypeSymbol.Bool),
            
            new BoundBinaryOperator(PlusToken, Addition, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Float64),
            new BoundBinaryOperator(DashToken, Subtraction, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Float64),
            new BoundBinaryOperator(StarToken, Multiplication, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Float64),
            new BoundBinaryOperator(SlashToken, Division, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Float64),
            new BoundBinaryOperator(DoubleStarToken, Exponentiation, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Float64),
            new BoundBinaryOperator(DoubleEqualsToken, LogicalEquals, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(BangEqualsToken, LogicalNotEquals, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(GreaterThanToken, GreaterThan, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(GreaterThanOrEqualToken, GreaterThanOrEqual, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(LessThanToken, LessThan, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(LessThanOrEqualToken, LessThanOrEqual, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            
            new BoundBinaryOperator(PlusToken, Addition, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Float64),
            new BoundBinaryOperator(DashToken, Subtraction, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Float64),
            new BoundBinaryOperator(StarToken, Multiplication, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Float64),
            new BoundBinaryOperator(SlashToken, Division, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Float64),
            new BoundBinaryOperator(DoubleStarToken, Exponentiation, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Float64),
            new BoundBinaryOperator(DoubleEqualsToken, LogicalEquals, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(BangEqualsToken, LogicalNotEquals, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(GreaterThanToken, GreaterThan, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(GreaterThanOrEqualToken, GreaterThanOrEqual, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(LessThanToken, LessThan, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(LessThanOrEqualToken, LessThanOrEqual, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
        };

        public static BoundBinaryOperator? Bind(TokType tokType, TypeSymbol leftType, TypeSymbol rightType)
        {
            return Operators.ToImmutableArray().FirstOrDefault(op =>
                op.TokType == tokType && op.LeftType == leftType && op.RightType == rightType);
        }
    }
}