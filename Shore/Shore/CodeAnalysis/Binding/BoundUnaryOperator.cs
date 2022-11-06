using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryOperator
    {
        public TokType TokType { get; }
        public BoundUnaryOperatorKind Kind { get; }
        public Type OperandType { get; }
        public Type ResultType { get; }

        private BoundUnaryOperator(TokType tokType, BoundUnaryOperatorKind kind, Type operandType)
            : this(tokType, kind, operandType, operandType)
        {
        }

        private BoundUnaryOperator(TokType tokType, BoundUnaryOperatorKind kind, Type operandType, Type resultType)
        {
            TokType = tokType;
            Kind = kind;
            OperandType = operandType;
            ResultType = resultType;
        }
        
        private static readonly BoundUnaryOperator[] Operators =
        {
            new (TokType.PlusToken, BoundUnaryOperatorKind.Identity, typeof(int)),
            new (TokType.DashToken, BoundUnaryOperatorKind.Negation, typeof(int)),
            new (TokType.TildeToken, BoundUnaryOperatorKind.OnesComplement, typeof(int)),
            
            new (TokType.BangToken, BoundUnaryOperatorKind.LogicalNegation, typeof(bool)),
        };

        public static BoundUnaryOperator? Bind(TokType tokType, Type operandType)
        {
            foreach (var op in Operators)
            {
                if (op.TokType == tokType && op.OperandType == operandType) return op;
            }

            return null;
        }
    }
}