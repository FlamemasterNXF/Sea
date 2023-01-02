using System.Text;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.Text;

namespace Shore.CodeAnalysis.Syntax
{
    internal class Lexer
    {
        private readonly NodeTree _nodeTree;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SourceText _text;
        
        private int _position;
        private int _start;
        private TokType _type;
        private object? _value;

        public Lexer(NodeTree nodeTree)
        {
            _nodeTree = nodeTree;
            _text = nodeTree.Text;
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
                    _position++;
                    if (Current != '*') _type = TokType.StarToken;
                    else
                    {
                        _type = TokType.DoubleStarToken;
                        _position++;
                    }
                    break;
                case '/':
                    if (Lookahead == '/') ReadSingleLineComment();
                    else if (Lookahead == '*') ReadMultiLineComment();
                    else
                    {
                        _type = TokType.SlashToken;
                        _position++;
                    }
                    break;              
                case '(':
                    _type = TokType.OpenParenToken;
                    _position++;
                    break;              
                case ')':
                    _type = TokType.CloseParenToken;
                    _position++;
                    break;    
                case '{':
                    _type = TokType.OpenBraceToken;
                    _position++;
                    break;              
                case '}':
                    _type = TokType.CloseBraceToken;
                    _position++;
                    break;
                case '[':
                    _type = TokType.OpenBracketToken;
                    _position++;
                    break;              
                case ']':
                    _type = TokType.CloseBracketToken;
                    _position++;
                    break;
                case ',':
                    _type = TokType.CommaToken;
                    _position++;
                    break;
                case '~':
                    _type = TokType.TildeToken;
                    _position++;
                    break;
                case '^':
                    _type = TokType.CaratToken;
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
                    _position++;
                    if (Current != '&') _type = TokType.AmpersandToken;
                    else
                    {
                        _type = TokType.DoubleAmpersandToken;
                        _position++;
                    }
                    break;
                case '|':
                    _position++;
                    if (Current != '|') _type = TokType.PipeToken;
                    else
                    {
                        _type = TokType.DoublePipeToken;
                        _position++;
                    }
                    break;
                case '<':
                    _position++;
                    switch (Current)
                    {
                        case '=':
                            _type = TokType.LessThanOrEqualToken;
                            _position++;
                            break;
                        case '<': 
                            _type = TokType.LeftShiftToken;
                            _position++;
                            break;
                        default:
                            _type = TokType.LessThanToken;
                            break;
                    }
                    break;
                case '>':
                    _position++;
                    switch (Current)
                    {
                        case '=':
                            _type = TokType.GreaterThanOrEqualToken;
                            _position++;
                            break;
                        case '>': 
                            _type = TokType.RightShiftToken;
                            _position++;
                            break;
                        default:
                            _type = TokType.GreaterThanToken;
                            break;
                    }
                    break;
                case '"':
                    ReadString();
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
                        var span = new TextSpan(_position, 1);
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportUnknownCharacter(location, Current);
                        _position++;
                    }
                    break;
            }

            var length = _position - _start;
            var text = SyntaxFacts.GetText(_type) ?? _text.ToString(_start, length);
            return new Token(_nodeTree, _type, _start, text, _value);
        }

        private void ReadWhiteSpace()
        {
            while (char.IsWhiteSpace(Current)) _position++;
            _type = TokType.WhitespaceToken;
        }

        private void ReadString()
        {
            _position++;

            var builder = new StringBuilder();
            var done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\0' or '\r' or '\n':
                        var span = new TextSpan(_start, 1);
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportUnterminatedString(location);
                        done = true;
                        break;
                    case '"':
                        if (Lookahead == '"')
                        {
                            builder.Append(Current);
                            _position += 2;
                        }
                        else
                        {
                            _position++;
                            done = true;
                        }
                        break;
                    default:
                        builder.Append(Current);
                        _position++;
                        break;
                }
            }

            _type = TokType.StringToken;
            _value = builder.ToString();
        }
        
        private void ReadNumberToken()
        {
            var hasDecimal = false;
            while (char.IsDigit(Current) || (Current == '.' && !hasDecimal))
            {
                if (Current == '.') hasDecimal = true;
                _position++;
            }

            var length = _position - _start;
            var text = _text.ToString(_start, length); 
            
            double.TryParse(text, out var temp);
            if (!text.Contains('.') && (Math.Abs(temp % 1)) <= (Double.Epsilon * 100))
            {
                if (!long.TryParse(text, out var value))
                {
                    var span = new TextSpan(_start, length);
                    var location = new TextLocation(_text, span);
                    _diagnostics.ReportInvalidNumber(location, text);
                }
                _value = value;
            }
            else
            {
                if (!double.TryParse(text, out var value))
                {
                    var span = new TextSpan(_start, length);
                    var location = new TextLocation(_text, span);
                    _diagnostics.ReportInvalidNumber(location, text);
                }   
                _value = value;
            }
            
            _type = TokType.NumberToken;
        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current) || char.IsNumber(Current)) _position++;
            var length = _position - _start;
            var text = _text.ToString(_start, length);
            
            // TODO: Clean up this garbage 
            if ((text is "int" or "float" or "bool" or "string") && Current == '[')
            {
                _position++;
                if (Current == ']')
                {
                    _position++;
                    text += "[]";
                }
            }
            if ((text is "int" or "float" or "bool" or "string") && Current == '<')
            {
                _position++;
                if (Current == '>')
                {
                    _position++;
                    text += "<>";
                }
            }
            
            _type = text.GetKeywordType();
        }

        private void ReadSingleLineComment()
        {
            _position += 2;
            var done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\r' or '\n' or '\0':
                        done = true;
                        break;
                    default:
                        _position++;
                        break;
                }
            }

            _type = TokType.SingleLineCommentToken;
        }

        private void ReadMultiLineComment()
        {
            _position += 2;
            var done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                        var span = new TextSpan(_start, 2);
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportUnterminatedMultiLineComment(location);
                        done = true;
                        break;
                    case '*':
                        if (Lookahead == '/')
                        {
                            _position++;
                            done = true;
                        }
                        _position++;
                        break;
                    default:
                        _position++;
                        break;
                }

                _type = TokType.MultiLineCommentToken;
            }
        }
    }
}