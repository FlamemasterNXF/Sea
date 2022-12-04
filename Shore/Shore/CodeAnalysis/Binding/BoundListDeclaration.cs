using System.Collections.Immutable;
using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundListDeclaration : BoundStatement
    {
        public VariableSymbol List { get; }
        public ImmutableArray<VariableSymbol> Members { get; }
        public ImmutableArray<BoundExpression> Values { get; }
        public override BoundNodeKind Kind => BoundNodeKind.ListDeclaration;

        public BoundListDeclaration(VariableSymbol list, ImmutableArray<VariableSymbol> members, ImmutableArray<BoundExpression> values)
        {
            List = list;
            Members = members;
            Values = values;
        }
    }
}