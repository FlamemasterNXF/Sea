namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundLabelStatement : BoundStatement
    {
        public BoundLabel BoundLabel { get; }
        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;

        public BoundLabelStatement(BoundLabel boundLabel)
        {
            BoundLabel = boundLabel;
        }
    }
}