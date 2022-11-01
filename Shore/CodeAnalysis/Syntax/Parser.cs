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

        private Token PeekToken(int offset)
        {
            var index = _position + offset;
            return index >= _tokens.Length ? _tokens[^1] : _tokens[index];
        }
        
        private Token CurrentToken => PeekToken(0);

        private Token NextToken()
        {
            var current = CurrentToken;
            _position++;
            return current;
        }
        
        private Token MatchToken(TokType type)
        {
            if (CurrentToken.Type == type) return NextToken();
            
            _diagnostics.Add($"ERROR: Unexpected Token <{CurrentToken.Type}>, {type} was expected.");
            return new Token(type, CurrentToken.Position, null, null);
        }

        private ExpressionNode ParsePrimaryExpression()
        {
            if (CurrentToken.Type == TokType.OpenParenToken)
            {
                var left = NextToken();
                var expression = ParseExpression();
                var right = MatchToken(TokType.CloseParenToken);
                return new ParenthesisExpressionNode(left, expression, right);
            }
            
            var numberToken = MatchToken(TokType.NumberToken);
            return new NumberExpressionNode(numberToken);
        }

        private ExpressionNode ParseTerm()
        {
            var left = ParseFactor();

            while (CurrentToken.Type is TokType.PlusToken or TokType.DashToken)
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

            while (CurrentToken.Type is TokType.StarToken or TokType.SlashToken)
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
            var expression = ParseExpression();
            var eof = MatchToken(TokType.EndOfFileToken);
            return new NodeTree(_diagnostics, expression, eof);
        }
    }
}