using System.Collections.Immutable;
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

        private static readonly List<BoundBinaryOperator> Operators = new()
        {
            new BoundBinaryOperator(TokType.CaratToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Int64),
            new BoundBinaryOperator(TokType.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Int64),
            new BoundBinaryOperator(TokType.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Int64),
            new BoundBinaryOperator(TokType.RightShiftToken, BoundBinaryOperatorKind.BitwiseRightShift, TypeSymbol.Int64),
            new BoundBinaryOperator(TokType.LeftShiftToken, BoundBinaryOperatorKind.BitwiseLeftShift, TypeSymbol.Int64),
            
            new BoundBinaryOperator(TokType.CaratToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.DoubleAmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.DoublePipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, TypeSymbol.Bool),
            
            new BoundBinaryOperator(TokType.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.String),
            new BoundBinaryOperator(TokType.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.String, TypeSymbol.Int64, TypeSymbol.String),
            new BoundBinaryOperator(TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, TypeSymbol.String, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, TypeSymbol.String, TypeSymbol.Bool),
            
            new BoundBinaryOperator(TokType.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Int64),
            new BoundBinaryOperator(TokType.DashToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.Int64),
            new BoundBinaryOperator(TokType.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Int64),
            new BoundBinaryOperator(TokType.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Int64),
            new BoundBinaryOperator(TokType.DoubleStarToken, BoundBinaryOperatorKind.Exponentiation, TypeSymbol.Int64),
            new BoundBinaryOperator(TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.GreaterThanToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.GreaterThanOrEqualToken, BoundBinaryOperatorKind.GreaterThanOrEqual, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.LessThanToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.LessThanOrEqualToken, BoundBinaryOperatorKind.LessThanOrEqual, TypeSymbol.Int64, TypeSymbol.Bool),
            
            new BoundBinaryOperator(TokType.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.DashToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.DoubleStarToken, BoundBinaryOperatorKind.Exponentiation, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.GreaterThanToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.GreaterThanOrEqualToken, BoundBinaryOperatorKind.GreaterThanOrEqual, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.LessThanToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.LessThanOrEqualToken, BoundBinaryOperatorKind.LessThanOrEqual, TypeSymbol.Float64, TypeSymbol.Bool),
            
            new BoundBinaryOperator(TokType.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.DashToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.DoubleStarToken, BoundBinaryOperatorKind.Exponentiation, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.GreaterThanToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.GreaterThanOrEqualToken, BoundBinaryOperatorKind.GreaterThanOrEqual, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.LessThanToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.LessThanOrEqualToken, BoundBinaryOperatorKind.LessThanOrEqual, TypeSymbol.Float64, TypeSymbol.Int64, TypeSymbol.Bool),
            
            new BoundBinaryOperator(TokType.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.DashToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.DoubleStarToken, BoundBinaryOperatorKind.Exponentiation, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Float64),
            new BoundBinaryOperator(TokType.DoubleEqualsToken, BoundBinaryOperatorKind.LogicalEquals, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.BangEqualsToken, BoundBinaryOperatorKind.LogicalNotEquals, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.GreaterThanToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.GreaterThanOrEqualToken, BoundBinaryOperatorKind.GreaterThanOrEqual, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.LessThanToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
            new BoundBinaryOperator(TokType.LessThanOrEqualToken, BoundBinaryOperatorKind.LessThanOrEqual, TypeSymbol.Int64, TypeSymbol.Float64, TypeSymbol.Bool),
        };

        public static BoundBinaryOperator? Bind(TokType tokType, TypeSymbol leftType, TypeSymbol rightType)
        {
            return Operators.ToImmutableArray().FirstOrDefault(op =>
                op.TokType == tokType && op.LeftType == leftType && op.RightType == rightType);
        }
    }
}