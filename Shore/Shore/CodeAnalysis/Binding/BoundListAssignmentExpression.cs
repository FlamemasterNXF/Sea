using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundListAssignmentExpression : BoundExpression
    {
        public VariableSymbol? Variable { get; }
        public BoundExpression Expression { get; }
        public BoundExpression Accessor { get; }

        public BoundListAssignmentExpression(VariableSymbol? variable, BoundExpression expression,
            BoundExpression accessor)
        {
            Variable = variable;
            Expression = expression;
            Accessor = accessor;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ListAssignmentExpression;
        public override TypeSymbol? Type => Expression.Type;
    }
}