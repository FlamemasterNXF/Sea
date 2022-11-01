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

            var parser = new Parser(line);
            var expression = parser.Parse();
            
            LogNode(expression);

            // CONSOLE WINDOW CONTROL
            //Console.WriteLine("Press C to close this window :)");
            //try{ while(Console.ReadKey(true).Key != ConsoleKey.C){ Console.Read(); } }
            //catch (Exception){ Console.WriteLine($"Console Window not found!"); }
        }

        static void LogNode(Node node, string indent = "", bool last = false)
        {
            var marker = last ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Type);

            if (node is Token t && t.Value is not null)
            {
                Console.Write(" ");
                Console.Write(t.Value);   
            }
            
            Console.WriteLine();
            indent += last ? "    " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
            {
                LogNode(child, indent, child == lastChild);
            }
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
        EndOfFileToken,
        NumberExpression,
        BinaryExpression
    }
    
    class Token : Node
    {
        public override TokType Type { get; }

        public override IEnumerable<Node> GetChildren()
        {
            return Enumerable.Empty<Node>();
        }

        public int Position { get; }
        public string? Text { get; }
        public object? Value { get; }

        public Token(TokType type, int position, string? text, object? value)
        {
            Type = type;
            Position = position;
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
                return new Token(TokType.EndOfFileToken, _position, "\0", null);

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

    abstract class Node
    {
        public abstract TokType Type { get; }

        public abstract IEnumerable<Node> GetChildren();
    }

    abstract class ExpressionNode : Node
    {
        
    }

    sealed class NumberExpressionNode : ExpressionNode
    {
        public Token NumberToken { get; }

        public NumberExpressionNode(Token numberToken)
        {
            NumberToken = numberToken;
        }

        public override TokType Type => TokType.NumberExpression;
        public override IEnumerable<Node> GetChildren()
        {
            yield return NumberToken;
        }
    }

    sealed class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; }
        public Token OperatorToken { get; }
        public ExpressionNode Right { get; }

        public BinaryExpressionNode(ExpressionNode left, Token operatorToken, ExpressionNode right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }

        public override TokType Type => TokType.BinaryExpression;
        public override IEnumerable<Node> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }
    
    class Parser
    {
        private readonly Token[] _tokens;
        private int _position;

        public Parser(string text)
        {
            var tokens = new List<Token>();

            var lexer = new Lexer(text);
            Token token;

            do 
            {
                token = lexer.NextToken();
                if (token.Type != TokType.WhitespaceToken && token.Type != TokType.UnknownToken) tokens.Add(token);
            } while (token.Type != TokType.EndOfFileToken);

            _tokens = tokens.ToArray();
        }

        private Token Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length) return _tokens[^1];
            return _tokens[index];
        }
        
        private Token Current => Peek(0);

        private Token NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }
        
        private Token Match(TokType type)
        {
            if (Current.Type == type) return NextToken();
            return new Token(type, Current.Position, null, null);
        }

        private ExpressionNode ParsePrimaryExpression()
        {
            var numberToken = Match(TokType.NumberToken);
            return new NumberExpressionNode(numberToken);
        }
        
        public ExpressionNode Parse()
        {
            var left = ParsePrimaryExpression();

            while (Current.Type is TokType.PlusToken or TokType.DashToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionNode(left, operatorToken, right);
            }

            return left;
        }
    }
}

