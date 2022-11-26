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
        
        private static readonly List<BoundUnaryOperator> FixedOperators = new()
        {
            new (TokType.TildeToken, BoundUnaryOperatorKind.OnesComplement, TypeSymbol.Int64),
            
            new (TokType.BangToken, BoundUnaryOperatorKind.LogicalNegation, TypeSymbol.Bool),
        };

        private static BoundUnaryOperator[] Operators()
        {
            List<BoundUnaryOperator> dynamicOperators = new List<BoundUnaryOperator>();
            foreach (var type in TypeSymbol.GetChildrenTypes(TypeSymbol.Integer))
            {
                dynamicOperators.Add(new BoundUnaryOperator(TokType.PlusToken, BoundUnaryOperatorKind.Identity, type));
                dynamicOperators.Add(new BoundUnaryOperator(TokType.DashToken, BoundUnaryOperatorKind.Negation, type));
            }
            foreach (var type in TypeSymbol.GetChildrenTypes(TypeSymbol.Float))
            {
                dynamicOperators.Add(new BoundUnaryOperator(TokType.PlusToken, BoundUnaryOperatorKind.Identity, type));
                dynamicOperators.Add(new BoundUnaryOperator(TokType.DashToken, BoundUnaryOperatorKind.Negation, type));
            }

            var operators = dynamicOperators.Concat(FixedOperators);
            return operators.ToArray();
        }

        public static BoundUnaryOperator? Bind(TokType tokType, TypeSymbol? operandType)
        {
            foreach (var op in Operators())
            {
                if (op.TokType == tokType && op.OperandType == operandType) return op;
            }

            return null;
        }
    }
}