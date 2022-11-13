using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundAssignmentExpression : BoundExpression
    {
        public VariableSymbol? Variable { get; }
        public BoundExpression? Expression { get; }

        public BoundAssignmentExpression(VariableSymbol? variable, BoundExpression? expression)
        {
            Variable = variable;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
        public override TypeSymbol? Type => Expression.Type;
    }
}