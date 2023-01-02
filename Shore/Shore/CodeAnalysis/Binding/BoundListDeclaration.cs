using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundListDeclaration : BoundStatement
    {
        public VariableSymbol Array { get; }
        public Dictionary<VariableSymbol, BoundExpression> Members { get; }
        public override BoundNodeKind Kind => BoundNodeKind.ListDeclaration;

        public BoundListDeclaration(VariableSymbol array, Dictionary<VariableSymbol, BoundExpression> members)
        {
            Array = array;
            Members = members;
        }
    }
}