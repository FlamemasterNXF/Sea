using System.Collections.Immutable;
using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundDictDeclaration : BoundStatement
    {
        public VariableSymbol Array { get; }
        public Dictionary<BoundExpression, BoundExpression> Values { get; }
        public override BoundNodeKind Kind => BoundNodeKind.DictDeclaration;

        public BoundDictDeclaration(VariableSymbol array, Dictionary<BoundExpression, BoundExpression> values)

        {
            Array = array;
            Values = values;
        }
    }
}