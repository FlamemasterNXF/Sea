using Shore.CodeAnalysis.Nodes;

namespace Shore.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly Token[] _tokens;

        private readonly List<string> _diagnostics = new List<string>();
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
            return index >= _tokens.Length ? _tokens[^1] : _tokens[index];
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
}