using Shore.Text;

namespace Shore
{
    internal class TextSpanComparer : IComparer<TextSpan>
    {
        public int Compare(TextSpan x, TextSpan y)
        {
            var diff = x.Start - y.Start;
            if (diff == 0) diff = x.Length - y.Length;
            return diff;
        }
    }
}