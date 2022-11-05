namespace Shore.Text
{
    public readonly struct TextSpan
    {
        public int Start { get; }
        public int Length { get; }

        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int End => Start + Length;

        public static TextSpan FromBounds(int start, int end)
        {
            var length = end - start;
            return new TextSpan(start, length);
        }
    }
}