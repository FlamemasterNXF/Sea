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

        private ExpressionNode ParseExpression(int parentPrecedence = 0)
        {
            var left = ParsePrimaryExpression();

            while (true)
            {
                var precedence = GetBinaryOperatorPrecedence(CurrentToken.Type);
                if (precedence == 0 || precedence <= parentPrecedence) break;

                var operatorToken = NextToken();
                var right = ParseExpression(precedence);
                left = new BinaryExpressionNode(left, operatorToken, right);
            }

            return left;
        }

        private static int GetBinaryOperatorPrecedence(TokType type)
        {
            switch (type)
            {
                case TokType.StarToken:
                case TokType.SlashToken: 
                    return 2;
                
                case TokType.PlusToken:
                case TokType.DashToken:
                    return 1;

                default: return 0;
            }
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

        public NodeTree Parse()
        {
            var expression = ParseExpression();
            var eof = MatchToken(TokType.EndOfFileToken);
            return new NodeTree(_diagnostics, expression, eof);
        }
    }
}