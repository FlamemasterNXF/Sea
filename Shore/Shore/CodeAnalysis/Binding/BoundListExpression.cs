using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundDictExpression : BoundExpression
    {
        public VariableSymbol? Array { get; }
        public BoundExpression Accessor { get; }

        public BoundDictExpression(VariableSymbol? array, BoundExpression accessor)
        {
            Array = array;
            Accessor = accessor;
        }

        public override BoundNodeKind Kind => BoundNodeKind.DictExpression;
        public override TypeSymbol Type => TypeSymbol.GetAcceptedType(Array.Type);
    }
    internal sealed class BoundListExpression : BoundExpression
    {
        public VariableSymbol? Array { get; }
        public BoundExpression Accessor { get; }

        public BoundListExpression(VariableSymbol? array, BoundExpression accessor)
        {
            Array = array;
            Accessor = accessor;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ListExpression;
        public override TypeSymbol Type => TypeSymbol.GetAcceptedType(Array.Type);
    }
}