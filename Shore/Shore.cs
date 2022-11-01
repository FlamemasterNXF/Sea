using System;
using System.Text;
using System.Data;
namespace Shore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool showTree = false;

            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) return;

                if (line == "#SHOWTREE")
                {
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing Node Trees" : "Hiding Node Trees");
                    continue;
                }

                var tree = NodeTree.Parse(line);
                var color = Console.ForegroundColor;

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    LogNode(tree.Root);
                    Console.ForegroundColor = color;
                }

                if (tree.Diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var diagnostic in tree.Diagnostics) Console.WriteLine(diagnostic);
                    Console.ForegroundColor = color;
                }
                else
                {
                    var e = new Evaluator(tree.Root);
                    var result = e.Evaluate();
                    Console.WriteLine(result);
                }   
            }

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
        BinaryExpression,
        ParenthesisExpression
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
        private List<string> _diagnostics = new List<string>();

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

            if (Current == '+') return new Token(TokType.PlusToken, _position++, "+", null);
            if (Current == '-') return new Token(TokType.DashToken, _position++, "-", null);
            if (Current == '*') return new Token(TokType.StarToken, _position++, "*", null);
            if (Current == '/') return new Token(TokType.SlashToken, _position++, "/", null);
            if (Current == '(') return new Token(TokType.OpenParenToken, _position++, "(", null);
            if (Current == ')') return new Token(TokType.CloseParenToken, _position++, ")", null);
            
            _diagnostics.Add($"ERROR: Unknown Character: '{Current}'.");
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

    sealed class ParenthesisExpressionNode : ExpressionNode
    {
        public Token OpenParen { get; }
        public ExpressionNode Expression { get; }
        public Token CloseParen { get; }

        public ParenthesisExpressionNode(Token openParen, ExpressionNode expression, Token closeParen)
        {
            OpenParen = openParen;
            Expression = expression;
            CloseParen = closeParen;
        }

        public override TokType Type => TokType.ParenthesisExpression;
        public override IEnumerable<Node> GetChildren()
        {
            yield return OpenParen;
            yield return Expression;
            yield return CloseParen;
        }
    }

    sealed class NodeTree
    {
        public IReadOnlyList<string> Diagnostics { get; }
        public ExpressionNode Root { get; }
        public Token EndOfFileToken { get; }

        public NodeTree(IEnumerable<string> diagnostics, ExpressionNode root, Token endOfFileToken)
        {
            Diagnostics = diagnostics.ToArray();
            Root = root;
            EndOfFileToken = endOfFileToken;
        }

        public static NodeTree Parse(string text)
        {
            var parser = new Parser(text);
            return parser.Parse();
        }
    }
        
    class Parser
    {
        private readonly Token[] _tokens;

        private List<string> _diagnostics = new List<string>();
        private int _position;

        public Parser(string text)
        {
            var tokens = new List<Token>();

            var lexer = new Lexer(text);
            Token token;

            do {
                token = lexer.NextToken();
                if (token.Type != TokType.WhitespaceToken && token.Type != TokType.UnknownToken) tokens.Add(token);
            } while (token.Type != TokType.EndOfFileToken);

            _tokens = tokens.ToArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }
        
        public IEnumerable<string> Diagnostics => _diagnostics;

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
            
            _diagnostics.Add($"ERROR: Unexpected Token <{Current.Type}>, {type} was expected.");
            return new Token(type, Current.Position, null, null);
        }

        private ExpressionNode ParsePrimaryExpression()
        {
            if (Current.Type == TokType.OpenParenToken)
            {
                var left = NextToken();
                var expression = ParseExpression();
                var right = Match(TokType.CloseParenToken);
                return new ParenthesisExpressionNode(left, expression, right);
            }
            
            var numberToken = Match(TokType.NumberToken);
            return new NumberExpressionNode(numberToken);
        }

        private ExpressionNode ParseTerm()
        {
            var left = ParseFactor();

            while (Current.Type is TokType.PlusToken or TokType.DashToken)
            {
                var operatorToken = NextToken();
                var right = ParseFactor();
                left = new BinaryExpressionNode(left, operatorToken, right);
            }

            return left;
        }
        
        private ExpressionNode ParseFactor()
        {
            var left = ParsePrimaryExpression();

            while (Current.Type is TokType.StarToken or TokType.SlashToken)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionNode(left, operatorToken, right);
            }

            return left;
        }

        private ExpressionNode ParseExpression()
        {
            return ParseTerm();
        }
        
        public NodeTree Parse()
        {
            var expression = ParseTerm();
            var eof = Match(TokType.EndOfFileToken);
            return new NodeTree(_diagnostics, expression, eof);
        }
    }

    class Evaluator
    {
        private readonly ExpressionNode _root;
        public Evaluator(ExpressionNode root)
        {
            _root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(ExpressionNode node)
        {
            if (node is NumberExpressionNode n) return (int) n.NumberToken.Value;
            
            if (node is BinaryExpressionNode b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                if (b.OperatorToken.Type == TokType.PlusToken) return left + right;
                if (b.OperatorToken.Type == TokType.DashToken) return left - right; 
                if (b.OperatorToken.Type == TokType.StarToken) return left * right;
                if (b.OperatorToken.Type == TokType.SlashToken) return left / right;
                throw new Exception($"Unexpected Binary Operator '{b.OperatorToken.Type}'");
            }

            if (node is ParenthesisExpressionNode p) return EvaluateExpression(p.Expression);
            
            throw new Exception($"Unexpected Node '{node.Type}'");
        }
    }
}

