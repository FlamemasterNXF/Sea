using System;
using System.Text;
using System.Data;
namespace Shore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) return;
            
            var lexer = new Lexer(line);
            while (true)
            {
                var token = lexer.NextToken();
                if (token.Type == TokType.EOF)
                    break;
                Console.WriteLine($"{token.Type}: '{token.Text}'");
                if(token.Value is not null) Console.WriteLine($"  {token.Value}");
            }

            // CONSOLE WINDOW CONTROL
            // Console.WriteLine("Press C to close this window :)");
            // try{ while(Console.ReadKey(true).Key != ConsoleKey.C){ Console.Read(); } }
            // catch (Exception){ Console.WriteLine($"Console Window not found!"); }
        }
    }

    enum TokType
    {
        NumberToken,
        WhitespaceToken,
        DashToken,
        StarToken,
        SlashToken,
        PlusToken,
        CloseParenToken,
        OpenParenToken,
        UnknownToken,
        EOF
    }
    
    class Token
    {
        public TokType Type { get; }
        public int Pos { get; }
        public string Text { get; }
        public object? Value { get; }

        public Token(TokType type, int pos, string text, object? value)
        {
            Type = type;
            Pos = pos;
            Text = text;
            Value = value;
        }
    }

    class Lexer
    {
        private readonly string _text;
        private int _position;

        public Lexer(string text)
        {
            _text = text;
        }

        private char Current => _position >= _text.Length ? '\0' : _text[_position];

        private void Next()
        {
            _position++;
        }
        public Token NextToken()
        {
            if (_position >= _text.Length)
                return new Token(TokType.EOF, _position, "\0", null);

            if (char.IsDigit(Current))
            {
                var start = _position;

                while (char.IsDigit(Current))
                    Next();

                var length = _position - start;
                var text = _text.Substring(start, length);
                int.TryParse(text, out var value);
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

            if (Current == '+') return new Token(TokType.PlusToken, _position++, "+", null);
            if (Current == '-') return new Token(TokType.DashToken, _position++, "-", null);
            if (Current == '*') return new Token(TokType.StarToken, _position++, "*", null);
            if (Current == '/') return new Token(TokType.SlashToken, _position++, "/", null);
            if (Current == '(') return new Token(TokType.OpenParenToken, _position++, "(", null);
            if (Current == ')') return new Token(TokType.CloseParenToken, _position++, ")", null);

            return new Token(TokType.UnknownToken, _position++, _text.Substring(_position - 1, 1), null);
        }
    }
}

