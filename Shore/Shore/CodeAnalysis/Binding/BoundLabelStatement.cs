namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundLabelStatement : BoundStatement
    {
        public LabelSymbol Label { get; }
        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;

        public BoundLabelStatement(LabelSymbol label)
        {
            Label = label;
        }
    }
}