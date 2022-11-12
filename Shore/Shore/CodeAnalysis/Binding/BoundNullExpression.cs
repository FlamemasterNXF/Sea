using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundNullExpression : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.NullExpression;
        public override TypeSymbol? Type => TypeSymbol.Null;
    }
}