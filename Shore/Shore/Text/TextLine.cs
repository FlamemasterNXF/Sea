namespace Shore.Text
{
    public sealed class TextLine
    {
        public SourceText Text { get; }
        public int Start { get; }
        public int Length { get; }
        public int LengthWithBreak { get; }

        public TextLine(SourceText text, int start, int length, int lengthWithBreak)
        {
            Text = text;
            Start = start;
            Length = length;
            LengthWithBreak = lengthWithBreak;
        }

        public int End => Start + Length;
        public TextSpan Span => new TextSpan(Start, Length);
        public TextSpan SpanWithBreak => new TextSpan(Start, LengthWithBreak);
        public override string ToString() => Text.ToString(Span);
    }
}