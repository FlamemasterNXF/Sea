using System.Collections.Immutable;
using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundArrayDeclaration : BoundStatement
    {
        public VariableSymbol Array { get; }
        public ImmutableArray<BoundExpression> Members { get; }
        public override BoundNodeKind Kind => BoundNodeKind.ArrayDeclaration;

        public BoundArrayDeclaration(VariableSymbol array, ImmutableArray<BoundExpression> members)
        {
            Array = array;
            Members = members;
        }
    }
}