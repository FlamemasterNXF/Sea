using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundExtendStatement : BoundStatement
    {
        public FunctionSymbol Function { get; }
        public TypeSymbol Type { get; }

        public override BoundNodeKind Kind => BoundNodeKind.ExtendStatement;

        public BoundExtendStatement(FunctionSymbol function)
        {
            Function = function;
            Type = function.Type;
        }
    }
    internal sealed class BoundForStatement : BoundLoopStatement
    {
        public VariableSymbol? Variable { get; }
        public BoundExpression LowerBound { get; }
        public BoundExpression UpperBound { get; }
        public BoundStatement Body { get; }
        public override BoundNodeKind Kind => BoundNodeKind.ForStatement;

        public BoundForStatement(VariableSymbol? variable, BoundExpression lowerBound, BoundExpression upperBound,
            BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel) : base(breakLabel, continueLabel)
        {
            Variable = variable;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Body = body;
        }
    }
}