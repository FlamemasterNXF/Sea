using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore.CodeAnalysis.Syntax
{
    internal class Lexer
    {
        private readonly string _text;
        private int _position;
        private readonly List<string> _diagnostics = new List<string>();

        public Lexer(string text)
        {
            _text = text;
        }

        public IEnumerable<string> Diagnostics => _diagnostics;

        private char Current => _position >= _text.Length ? '\0' : _text[_position];

        private void Next()
        {
            _position++;
        }
        public Token NextToken()
        {
            if (_position >= _text.Length)
                return new Token(TokType.EndOfFileToken, _position, "\0", null);

            if (char.IsDigit(Current))
            {
                var start = _position;

                while (char.IsDigit(Current))
                    Next();

                var length = _position - start;
                var text = _text.Substring(start, length);
                if(!int.TryParse(text, out var value)) _diagnostics.Add($"ERROR: '{text}' is not an Int32 value.");
                
                return new Token(TokType.NumberToken, start, text, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                var start = _position;

                while (char.IsWhiteSpace(Current))
                    Next();

                var length = _position - start;
                var text = _text.Substring(start, length);
                return new Token(TokType.WhitespaceToken , start, text, null);
            }

            if (char.IsLetter(Current))
            {
                var start = _position;

                while (char.IsLetter(Current))
                    Next();

                var length = _position - start;
                var text = _text.Substring(start, length);
                var type = text.GetKeywordType();
                return new Token(type , start, text, null);
            }

            switch (Current)
            {
                case '+':
                    return new Token(TokType.PlusToken, _position++, "+", null);
                case '-':
                    return new Token(TokType.DashToken, _position++, "-", null);
                case '*':
                    return new Token(TokType.StarToken, _position++, "*", null);
                case '/':
                    return new Token(TokType.SlashToken, _position++, "/", null);
                case '(':
                    return new Token(TokType.OpenParenToken, _position++, "(", null);
                case ')':
                    return new Token(TokType.CloseParenToken, _position++, ")", null);
                default:
                    _diagnostics.Add($"ERROR: Unknown Character: '{Current}'.");
                    return new Token(TokType.UnknownToken, _position++, _text.Substring(_position - 1, 1), null);
            }
        }
    }
}