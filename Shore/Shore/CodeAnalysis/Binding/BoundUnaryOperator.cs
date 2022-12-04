using System.Collections.Immutable;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryOperator
    {
        public TokType TokType { get; }
        public BoundUnaryOperatorKind Kind { get; }
        public TypeSymbol OperandType { get; }
        public TypeSymbol ResultType { get; }

        private BoundUnaryOperator(TokType tokType, BoundUnaryOperatorKind kind, TypeSymbol operandType)
            : this(tokType, kind, operandType, operandType)
        {
        }

        private BoundUnaryOperator(TokType tokType, BoundUnaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType)
        {
            TokType = tokType;
            Kind = kind;
            OperandType = operandType;
            ResultType = resultType;
        }
        
        private static readonly List<BoundUnaryOperator> Operators = new()
        {
            new BoundUnaryOperator(TokType.TildeToken, BoundUnaryOperatorKind.OnesComplement, TypeSymbol.Int64),
            new BoundUnaryOperator(TokType.PlusToken, BoundUnaryOperatorKind.Identity, TypeSymbol.Int64),
            new BoundUnaryOperator(TokType.DashToken, BoundUnaryOperatorKind.Negation, TypeSymbol.Int64),
            
            new BoundUnaryOperator(TokType.PlusToken, BoundUnaryOperatorKind.Identity, TypeSymbol.Float64),
            new BoundUnaryOperator(TokType.DashToken, BoundUnaryOperatorKind.Negation, TypeSymbol.Float64),
            
            new BoundUnaryOperator(TokType.BangToken, BoundUnaryOperatorKind.LogicalNegation, TypeSymbol.Bool),
        };

        public static BoundUnaryOperator? Bind(TokType tokType, TypeSymbol? operandType) =>
            Enumerable.FirstOrDefault(Operators.ToImmutableArray(),
                op => op.TokType == tokType && op.OperandType == operandType);
    }
}