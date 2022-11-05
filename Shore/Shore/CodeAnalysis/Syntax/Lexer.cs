using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.Text;

namespace Shore.CodeAnalysis.Syntax
{
    internal class Lexer
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly string _text;
        
        private int _position;
        private int _start;
        private TokType _type;
        private object? _value;

        public Lexer(string text)
        {
            _text = text;
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private char Current => PeekToken(0);
        private char Lookahead => PeekToken(1);

        private char PeekToken(int offset)
        {
            var index = _position + offset;
            return index >= _text.Length ? '\0' : _text[index];
        }
        
        public Token Lex()
        {
            _start = _position;
            _type = TokType.UnknownToken;
            _value = null;

            switch (Current)
            {
                case '\0':
                    _type = TokType.EndOfFileToken;
                    break;
                case '+':
                    _type = TokType.PlusToken;
                    _position++;
                    break;
                case '-':
                    _type = TokType.DashToken;
                    _position++;
                    break;              
                case '*':
                    _type = TokType.StarToken;
                    _position++;
                    break;              
                case '/':
                    _type = TokType.SlashToken;
                    _position++;
                    break;              
                case '(':
                    _type = TokType.OpenParenToken;
                    _position++;
                    break;              
                case ')':
                    _type = TokType.CloseParenToken;
                    _position++;
                    break;              
                case '!':
                    _position++;
                    if (Current != '=') _type = TokType.BangToken;
                    else
                    {
                        _type = TokType.BangEqualsToken;
                        _position++;
                    }
                    break;
                case '=':
                    _position++;
                    if (Current != '=') _type = TokType.EqualsToken;
                    else
                    {
                        _position++;
                        _type = TokType.DoubleEqualsToken;
                    }
                    break;
                case '&':
                    if(Lookahead == '&')
                    {
                        _position += 2;
                        _type = TokType.DoubleAmpersandToken;
                    }
                    break;
                case '|':
                    if(Lookahead == '|')
                    {
                        _position += 2;
                        _type = TokType.DoublePipeToken;
                    }
                    break;
                case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                    ReadNumberToken();
                    break;
                case ' ' or '\t' or '\n' or '\r':
                    ReadWhiteSpace();
                    break;
                default:
                    if (char.IsLetter(Current)) ReadIdentifierOrKeyword();
                    else if (char.IsWhiteSpace(Current)) ReadWhiteSpace();
                    else
                    {
                        _diagnostics.ReportUnknownCharacter(new TextSpan(_position, 1), Current);
                        _position++;
                    }
                    break;
            }

            var length = _position - _start;
            var text = SyntaxFacts.GetText(_type) ?? _text.Substring(_start, length);
            return new Token(_type, _start, text, _value);
        }

        private void ReadWhiteSpace()
        {
            while (char.IsWhiteSpace(Current)) _position++;
            _type = TokType.WhitespaceToken;
        }

        private void ReadNumberToken()
        {
            while (char.IsDigit(Current)) _position++;
            var length = _position - _start;
            var text = _text.Substring(_start, length);
            if (!int.TryParse(text, out var value)) _diagnostics.ReportInvalidNumber(new TextSpan(_start, length), _text, typeof(int));
            _value = value;
            _type = TokType.NumberToken;
        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current)) _position++;
            var length = _position - _start;
            var text = _text.Substring(_start, length);
            _type = text.GetKeywordType();
        }
    }
}