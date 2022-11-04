using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly Token[] _tokens;

        private DiagnosticBag _diagnostics = new DiagnosticBag();
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
        
        public DiagnosticBag Diagnostics => _diagnostics;

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
            
            _diagnostics.ReportUnexpectedToken(CurrentToken.Span, CurrentToken.Type, type);
            return new Token(type, CurrentToken.Position, null, null);
        }

        private ExpressionNode ParseExpression(int parentPrecedence = 0)
        {
            ExpressionNode left;
            var unaryOperatorPrecedence = CurrentToken.Type.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseExpression(unaryOperatorPrecedence);
                left = new UnaryExpressionNode(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            while (true) 
            {
                var precedence = CurrentToken.Type.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence) break;

                var operatorToken = NextToken();
                var right = ParseExpression(precedence);
                left = new BinaryExpressionNode(left, operatorToken, right);
            }

            return left;
        }

        private ExpressionNode ParsePrimaryExpression()
        {
            switch (CurrentToken.Type)
            {
                case TokType.OpenParenToken:
                {
                    var left = NextToken();
                    var expression = ParseExpression();
                    var right = MatchToken(TokType.CloseParenToken);
                    return new ParenthesisExpressionNode(left, expression, right);
                }
                case TokType.TrueKeyword:
                case TokType.FalseKeyword:
                {
                    var keywordToken = NextToken();
                    var value = keywordToken.Type is TokType.TrueKeyword;
                    return new LiteralExpressionNode(keywordToken, value);
                }
                default:
                {
                    var numberToken = MatchToken(TokType.NumberToken);
                    return new LiteralExpressionNode(numberToken);
                }
            }
        }

        public NodeTree Parse()
        {
            var expression = ParseExpression();
            var eof = MatchToken(TokType.EndOfFileToken);
            return new NodeTree(_diagnostics, expression, eof);
        }
    }
}